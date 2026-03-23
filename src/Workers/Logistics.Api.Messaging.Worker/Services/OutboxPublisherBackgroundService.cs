using System.Text.Json;
using Logistics.Api.BuildingBlocks.Contracts;
using Logistics.Api.BuildingBlocks.Infrastructure.Messaging;
using Logistics.Api.Shipments.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Logistics.Api.Messaging.Worker.Services;

public sealed class OutboxPublisherBackgroundService : BackgroundService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private static readonly IReadOnlyDictionary<string, Type> EventTypes = new Dictionary<string, Type>(StringComparer.Ordinal)
    {
        [typeof(ShipmentCreatedIntegrationEvent).FullName!] = typeof(ShipmentCreatedIntegrationEvent),
        [typeof(ShipmentStatusChangedIntegrationEvent).FullName!] = typeof(ShipmentStatusChangedIntegrationEvent),
        [typeof(ShipmentAssignedToHubIntegrationEvent).FullName!] = typeof(ShipmentAssignedToHubIntegrationEvent),
        [typeof(WebhookDeliveryRequestedIntegrationEvent).FullName!] = typeof(WebhookDeliveryRequestedIntegrationEvent),
        [typeof(WebhookDeliveryRetriedIntegrationEvent).FullName!] = typeof(WebhookDeliveryRetriedIntegrationEvent)
    };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly OutboxPublisherOptions _options;
    private readonly ILogger<OutboxPublisherBackgroundService> _logger;

    public OutboxPublisherBackgroundService(
        IServiceScopeFactory scopeFactory,
        IPublishEndpoint publishEndpoint,
        IOptions<OutboxPublisherOptions> options,
        ILogger<OutboxPublisherBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _publishEndpoint = publishEndpoint;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var publishedCount = await PublishPendingMessagesAsync(stoppingToken);
                if (publishedCount == 0)
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
        var dbContext = scope.ServiceProvider.GetRequiredService<ShipmentsDbContext>();
        var now = DateTimeOffset.UtcNow;

        var messages = await dbContext.OutboxMessages
            .Where(x => x.Status == "Pending" && (!x.NextRetryAt.HasValue || x.NextRetryAt <= now))
            .OrderBy(x => x.OccurredOn)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
            await PublishAsync(message, dbContext, cancellationToken);

        if (messages.Count > 0)
            await dbContext.SaveChangesAsync(cancellationToken);

        return messages.Count;
    }

    private async Task PublishAsync(OutboxMessage message, ShipmentsDbContext dbContext, CancellationToken cancellationToken)
    {
        if (!EventTypes.TryGetValue(message.Type, out var eventType))
        {
            message.Status = "Failed";
            message.Error = $"Unsupported outbox message type '{message.Type}'.";
            message.ProcessedOn = DateTimeOffset.UtcNow;
            return;
        }

        try
        {
            var deserialized = JsonSerializer.Deserialize(message.Payload, eventType, SerializerOptions)
                ?? throw new InvalidOperationException($"Failed to deserialize outbox message {message.Id}.");

            await _publishEndpoint.Publish(deserialized, eventType, cancellationToken);

            message.Status = "Processed";
            message.ProcessedOn = DateTimeOffset.UtcNow;
            message.Error = null;
            message.NextRetryAt = null;
        }
        catch (Exception ex)
        {
            message.RetryCount += 1;
            message.Error = ex.Message;

            if (message.RetryCount >= _options.MaxRetryCount)
            {
                message.Status = "Failed";
                message.ProcessedOn = DateTimeOffset.UtcNow;
                message.NextRetryAt = null;
            }
            else
            {
                message.Status = "Pending";
                message.NextRetryAt = DateTimeOffset.UtcNow.Add(CalculateBackoff(message.RetryCount));
            }

            _logger.LogError(ex, "Failed to publish outbox message {OutboxMessageId}", message.Id);
        }
    }

    private TimeSpan CalculateBackoff(int retryCount)
    {
        var seconds = _options.BaseRetryDelaySeconds * Math.Pow(2, Math.Max(0, retryCount - 1));
        return TimeSpan.FromSeconds(Math.Min(seconds, _options.MaxRetryDelaySeconds));
    }
}