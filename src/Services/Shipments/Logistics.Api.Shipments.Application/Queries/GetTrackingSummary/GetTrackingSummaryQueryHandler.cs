using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;
using Logistics.Api.Shipments.Application.Errors;
using Logistics.Api.Shipments.Domain.Repositories;

namespace Logistics.Api.Shipments.Application.Queries.GetTrackingSummary;

internal sealed class GetTrackingSummaryQueryHandler
    : IQueryHandler<GetTrackingSummaryQuery, Result<TrackingSummaryResponse>>
{
    private readonly IShipmentRepository _shipmentRepository;

    public GetTrackingSummaryQueryHandler(IShipmentRepository shipmentRepository)
    {
        _shipmentRepository = shipmentRepository;
    }

    public async Task<Result<TrackingSummaryResponse>> Handle(
        GetTrackingSummaryQuery query,
        CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByTrackingCodeAsync(query.TrackingCode, cancellationToken);
        if (shipment is null)
            return Result<TrackingSummaryResponse>.Failure(ShipmentErrors.NotFound);

        var lastUpdatedAt = shipment.TrackingEvents.Count > 0
            ? shipment.TrackingEvents.Max(x => x.OccurredAt)
            : shipment.UpdatedAt;

        return Result<TrackingSummaryResponse>.Success(new TrackingSummaryResponse(
            TrackingCode: shipment.TrackingCode,
            ShipmentCode: shipment.ShipmentCode,
            CurrentStatus: shipment.CurrentStatus.ToString(),
            ReceiverName: shipment.ReceiverName,
            LastUpdatedAt: lastUpdatedAt));
    }
}