using System.Text.Json;
using Logistics.Api.BuildingBlocks.Infrastructure.Messaging;
using Logistics.Api.Notifications.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Api.Notifications.Infrastructure.Messaging;

public sealed class InboxIdempotencyFilter<TMessage> : IFilter<ConsumeContext<TMessage>>
    where TMessage : class
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly NotificationsDbContext _dbContext;
    private readonly ILogger<InboxIdempotencyFilter<TMessage>> _logger;

    public InboxIdempotencyFilter(NotificationsDbContext dbContext, ILogger<InboxIdempotencyFilter<TMessage>> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
    {
        var messageId = ResolveMessageId(context);
        if (!messageId.HasValue)
        {
            await next.Send(context);
            return;
        }

        var consumerName = ResolveConsumerName(context);
        var existing = await _dbContext.InboxMessages
            .FirstOrDefaultAsync(x => x.Id == messageId.Value && x.ConsumerName == consumerName, context.CancellationToken);

        if (existing?.ProcessedOn is not null)
            return;

        if (existing is null)
        {
            existing = new InboxMessage
            {
                Id = messageId.Value,
                ConsumerName = consumerName,
                Type = typeof(TMessage).FullName ?? typeof(TMessage).Name,
                Payload = JsonSerializer.Serialize(context.Message, SerializerOptions),
                ReceivedOn = DateTimeOffset.UtcNow
            };

            _dbContext.InboxMessages.Add(existing);

            try
            {
                await _dbContext.SaveChangesAsync(context.CancellationToken);
            }
            catch (DbUpdateException ex) when (IsDuplicateKey(ex))
            {
                _logger.LogWarning("Duplicate message {MessageId} skipped for {ConsumerName}", messageId.Value, consumerName);
                return;
            }
        }

        try
        {
            await next.Send(context);
            existing.ProcessedOn = DateTimeOffset.UtcNow;
            existing.Error = null;
            await _dbContext.SaveChangesAsync(context.CancellationToken);
        }
        catch (Exception ex)
        {
            existing.Error = ex.Message;
            await _dbContext.SaveChangesAsync(context.CancellationToken);
            throw;
        }
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("notificationsInboxIdempotency");
    }

    private static Guid? ResolveMessageId(ConsumeContext<TMessage> context)
    {
        if (context.MessageId.HasValue)
            return context.MessageId.Value;

        var property = typeof(TMessage).GetProperty("EventId");
        if (property?.PropertyType == typeof(Guid))
            return (Guid?)property.GetValue(context.Message);

        if (property?.PropertyType == typeof(Guid?))
            return (Guid?)property.GetValue(context.Message);

        return null;
    }

    private static string ResolveConsumerName(ConsumeContext<TMessage> context)
        => context.ReceiveContext.InputAddress.AbsolutePath.Trim('/');

    private static bool IsDuplicateKey(DbUpdateException exception)
    {
        var message = (exception.InnerException ?? exception).Message;
        return message.Contains("23505") || message.Contains("inbox_messages", StringComparison.OrdinalIgnoreCase);
    }
}