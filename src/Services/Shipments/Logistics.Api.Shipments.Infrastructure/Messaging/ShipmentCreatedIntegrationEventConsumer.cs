using Logistics.Api.BuildingBlocks.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Logistics.Api.Shipments.Infrastructure.Messaging;

public sealed class ShipmentCreatedIntegrationEventConsumer : IConsumer<ShipmentCreatedIntegrationEvent>
{
    private readonly ILogger<ShipmentCreatedIntegrationEventConsumer> _logger;

    public ShipmentCreatedIntegrationEventConsumer(ILogger<ShipmentCreatedIntegrationEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ShipmentCreatedIntegrationEvent> context)
    {
        _logger.LogInformation(
            "Consumed ShipmentCreatedIntegrationEvent for shipment {ShipmentId}",
            context.Message.Payload.ShipmentId);

        return Task.CompletedTask;
    }
}