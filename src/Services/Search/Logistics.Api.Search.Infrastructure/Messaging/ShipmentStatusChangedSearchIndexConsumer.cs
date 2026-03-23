using Logistics.Api.BuildingBlocks.Contracts;
using Logistics.Api.Search.Infrastructure.Models;
using Logistics.Api.Search.Infrastructure.Services;
using Logistics.Api.Shipments.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Api.Search.Infrastructure.Messaging;

public sealed class ShipmentStatusChangedSearchIndexConsumer : IConsumer<ShipmentStatusChangedIntegrationEvent>
{
    private readonly ShipmentsDbContext _shipmentsDbContext;
    private readonly IShipmentSearchIndexService _indexService;
    private readonly ILogger<ShipmentStatusChangedSearchIndexConsumer> _logger;

    public ShipmentStatusChangedSearchIndexConsumer(
        ShipmentsDbContext shipmentsDbContext,
        IShipmentSearchIndexService indexService,
        ILogger<ShipmentStatusChangedSearchIndexConsumer> logger)
    {
        _shipmentsDbContext = shipmentsDbContext;
        _indexService = indexService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ShipmentStatusChangedIntegrationEvent> context)
    {
        var shipment = await _shipmentsDbContext.Shipments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == context.Message.Payload.ShipmentId, context.CancellationToken);

        if (shipment is null)
        {
            _logger.LogWarning("Shipment {ShipmentId} not found for search reindex after status change", context.Message.Payload.ShipmentId);
            return;
        }

        await _indexService.UpsertShipmentAsync(ShipmentSearchDocument.FromShipment(shipment), context.CancellationToken);
    }
}