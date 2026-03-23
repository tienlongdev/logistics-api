using System.Text.Json;
using Logistics.Api.BuildingBlocks.Contracts;
using Logistics.Api.Notifications.Application.Common;
using Logistics.Api.Notifications.Infrastructure.Persistence;
using Logistics.Api.Notifications.Infrastructure.Persistence.Entities;
using Logistics.Api.Notifications.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Api.Notifications.Infrastructure.Messaging;

public sealed class ShipmentCreatedIntegrationEventConsumer : IConsumer<ShipmentCreatedIntegrationEvent>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly NotificationsDbContext _dbContext;
    private readonly WebhookDeliveryWorkerOptions _options;
    private readonly ILogger<ShipmentCreatedIntegrationEventConsumer> _logger;

    public ShipmentCreatedIntegrationEventConsumer(
        NotificationsDbContext dbContext,
        IOptions<WebhookDeliveryWorkerOptions> options,
        ILogger<ShipmentCreatedIntegrationEventConsumer> logger)
    {
        _dbContext = dbContext;
        _options = options.Value;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ShipmentCreatedIntegrationEvent> context)
    {
        var subscriptions = await _dbContext.WebhookSubscriptions
            .Where(x => x.MerchantId == context.Message.Payload.MerchantId && x.IsActive)
            .Where(x => x.Events.Contains(WebhookEventNames.ShipmentCreated))
            .ToListAsync(context.CancellationToken);

        if (subscriptions.Count == 0)
            return;

        var payload = JsonSerializer.Serialize(context.Message, SerializerOptions);
        var now = DateTimeOffset.UtcNow;

        foreach (var subscription in subscriptions)
        {
            _dbContext.WebhookDeliveries.Add(new WebhookDeliveryEntity
            {
                Id = Guid.NewGuid(),
                SubscriptionId = subscription.Id,
                MerchantId = subscription.MerchantId,
                EventType = WebhookEventNames.ShipmentCreated,
                EventId = context.Message.EventId,
                Payload = payload,
                Status = "Pending",
                AttemptCount = 0,
                MaxAttempts = _options.MaxAttempts,
                NextRetryAt = now,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        try
        {
            await _dbContext.SaveChangesAsync(context.CancellationToken);
        }
        catch (DbUpdateException ex) when ((ex.InnerException ?? ex).Message.Contains("23505"))
        {
            _logger.LogWarning("Webhook deliveries already exist for event {EventId}", context.Message.EventId);
        }
    }
}