using Logistics.Api.BuildingBlocks.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Logistics.Api.Shipments.Infrastructure.Messaging;

public sealed class ShipmentStatusChangedIntegrationEventConsumer : IConsumer<ShipmentStatusChangedIntegrationEvent>
{
    private readonly ILogger<ShipmentStatusChangedIntegrationEventConsumer> _logger;

    public ShipmentStatusChangedIntegrationEventConsumer(ILogger<ShipmentStatusChangedIntegrationEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ShipmentStatusChangedIntegrationEvent> context)
    {
        _logger.LogInformation(
            "Consumed ShipmentStatusChangedIntegrationEvent for shipment {ShipmentId}: {FromStatus} -> {ToStatus}",
            context.Message.Payload.ShipmentId,
            context.Message.Payload.FromStatus,
            context.Message.Payload.ToStatus);

        return Task.CompletedTask;
    }
}