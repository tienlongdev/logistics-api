using Logistics.Api.BuildingBlocks.Contracts;
using Logistics.Api.Search.Infrastructure.Models;
using Logistics.Api.Search.Infrastructure.Services;
using Logistics.Api.Shipments.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Api.Search.Infrastructure.Messaging;

public sealed class ShipmentCreatedSearchIndexConsumer : IConsumer<ShipmentCreatedIntegrationEvent>
{
    private readonly ShipmentsDbContext _shipmentsDbContext;
    private readonly IShipmentSearchIndexService _indexService;
    private readonly ILogger<ShipmentCreatedSearchIndexConsumer> _logger;

    public ShipmentCreatedSearchIndexConsumer(
        ShipmentsDbContext shipmentsDbContext,
        IShipmentSearchIndexService indexService,
        ILogger<ShipmentCreatedSearchIndexConsumer> logger)
    {
        _shipmentsDbContext = shipmentsDbContext;
        _indexService = indexService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ShipmentCreatedIntegrationEvent> context)
    {
        await ReindexShipmentAsync(context.Message.Payload.ShipmentId, context.CancellationToken);
    }

    private async Task ReindexShipmentAsync(Guid shipmentId, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentsDbContext.Shipments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == shipmentId, cancellationToken);

        if (shipment is null)
        {
            _logger.LogWarning("Shipment {ShipmentId} not found for search indexing", shipmentId);
            return;
        }

        await _indexService.UpsertShipmentAsync(ShipmentSearchDocument.FromShipment(shipment), cancellationToken);
    }
}