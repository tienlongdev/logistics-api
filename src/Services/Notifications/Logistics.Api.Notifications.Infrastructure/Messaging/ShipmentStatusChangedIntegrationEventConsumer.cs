using System.Text.Json;
using Logistics.Api.BuildingBlocks.Contracts;
using Logistics.Api.Notifications.Application.Common;
using Logistics.Api.Notifications.Infrastructure.Persistence;
using Logistics.Api.Notifications.Infrastructure.Persistence.Entities;
using Logistics.Api.Notifications.Infrastructure.Services;
using Logistics.Api.Shipments.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Api.Notifications.Infrastructure.Messaging;

public sealed class ShipmentStatusChangedIntegrationEventConsumer : IConsumer<ShipmentStatusChangedIntegrationEvent>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly NotificationsDbContext _dbContext;
    private readonly ShipmentsDbContext _shipmentsDbContext;
    private readonly WebhookDeliveryWorkerOptions _options;
    private readonly ILogger<ShipmentStatusChangedIntegrationEventConsumer> _logger;

    public ShipmentStatusChangedIntegrationEventConsumer(
        NotificationsDbContext dbContext,
        ShipmentsDbContext shipmentsDbContext,
        IOptions<WebhookDeliveryWorkerOptions> options,
        ILogger<ShipmentStatusChangedIntegrationEventConsumer> logger)
    {
        _dbContext = dbContext;
        _shipmentsDbContext = shipmentsDbContext;
        _options = options.Value;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ShipmentStatusChangedIntegrationEvent> context)
    {
        var merchantId = await _shipmentsDbContext.Shipments
            .Where(x => x.Id == context.Message.Payload.ShipmentId)
            .Select(x => (Guid?)x.MerchantId)
            .FirstOrDefaultAsync(context.CancellationToken);

        if (!merchantId.HasValue)
        {
            _logger.LogWarning(
                "Unable to resolve merchant for shipment status event {EventId} shipment {ShipmentId}",
                context.Message.EventId,
                context.Message.Payload.ShipmentId);
            return;
        }

        var subscriptions = await _dbContext.WebhookSubscriptions
            .Where(x => x.MerchantId == merchantId.Value && x.IsActive)
            .Where(x => x.Events.Contains(WebhookEventNames.ShipmentStatusChanged))
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
                EventType = WebhookEventNames.ShipmentStatusChanged,
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