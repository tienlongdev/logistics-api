using System.Reflection;
using System.Text.Json;
using Logistics.Api.BuildingBlocks.Infrastructure.Messaging;
using Logistics.Api.BuildingBlocks.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Api.Shipments.Infrastructure.Messaging;

public sealed class OutboxPublisherBackgroundService : BackgroundService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private static readonly Assembly ContractsAssembly = typeof(IntegrationEventEnvelope<>).Assembly;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly MessagingWorkerOptions _options;
    private readonly ILogger<OutboxPublisherBackgroundService> _logger;

    public OutboxPublisherBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<MessagingWorkerOptions> options,
        ILogger<OutboxPublisherBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processed = await PublishPendingMessagesAsync(stoppingToken);
                if (processed == 0)
                    await Task.Delay(TimeSpan.FromSeconds(_options.PollIntervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox publisher loop failed");
                await Task.Delay(TimeSpan.FromSeconds(_options.PollIntervalSeconds), stoppingToken);
            }
        }
    }

    private async Task<int> PublishPendingMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Persistence.ShipmentsDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        var now = DateTimeOffset.UtcNow;

        var pendingMessages = await dbContext.OutboxMessages
            .Where(x => x.Status == "Pending" && (!x.NextRetryAt.HasValue || x.NextRetryAt <= now))
            .OrderBy(x => x.OccurredOn)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var outboxMessage in pendingMessages)
            await PublishSingleMessageAsync(dbContext, publishEndpoint, outboxMessage, cancellationToken);

        if (pendingMessages.Count > 0)
            await dbContext.SaveChangesAsync(cancellationToken);

        return pendingMessages.Count;
    }

    private async Task PublishSingleMessageAsync(
        Persistence.ShipmentsDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        OutboxMessage outboxMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            var messageType = ResolveMessageType(outboxMessage.Type);
            if (messageType is null)
                throw new InvalidOperationException($"Unable to resolve integration event type '{outboxMessage.Type}'.");

            var deserializedMessage = JsonSerializer.Deserialize(outboxMessage.Payload, messageType, SerializerOptions)
                ?? throw new InvalidOperationException($"Unable to deserialize outbox payload '{outboxMessage.Id}'.");

            await publishEndpoint.Publish(deserializedMessage, messageType, publishContext =>
            {
                publishContext.MessageId = outboxMessage.Id;
                if (outboxMessage.CorrelationId.HasValue)
                    publishContext.CorrelationId = outboxMessage.CorrelationId.Value;
            }, cancellationToken);

            outboxMessage.Status = "Processed";
            outboxMessage.ProcessedOn = DateTimeOffset.UtcNow;
            outboxMessage.Error = null;
            outboxMessage.NextRetryAt = null;
        }
        catch (Exception ex)
        {
            outboxMessage.RetryCount += 1;
            outboxMessage.Error = ex.Message;
            outboxMessage.ProcessedOn = null;

            if (outboxMessage.RetryCount >= _options.MaxRetryCount)
            {
                outboxMessage.Status = "Failed";
                outboxMessage.NextRetryAt = null;
            }
            else
            {
                outboxMessage.Status = "Pending";
                outboxMessage.NextRetryAt = DateTimeOffset.UtcNow.Add(CalculateBackoff(outboxMessage.RetryCount));
            }

            _logger.LogError(ex,
                "Failed to publish outbox message {OutboxMessageId}. RetryCount={RetryCount}",
                outboxMessage.Id,
                outboxMessage.RetryCount);
        }
    }

    private Type? ResolveMessageType(string typeName)
    {
        return ContractsAssembly.GetTypes().FirstOrDefault(x =>
                   x.FullName == typeName ||
                   x.AssemblyQualifiedName == typeName)
               ?? Type.GetType(typeName, throwOnError: false);
    }

    private TimeSpan CalculateBackoff(int retryCount)
    {
        var exponentialSeconds = _options.BaseRetryDelaySeconds * Math.Pow(2, retryCount - 1);
        var cappedSeconds = Math.Min(exponentialSeconds, _options.MaxRetryDelaySeconds);
        return TimeSpan.FromSeconds(cappedSeconds);
    }
}