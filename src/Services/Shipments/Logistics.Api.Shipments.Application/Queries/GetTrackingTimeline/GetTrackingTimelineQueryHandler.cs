using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;
using Logistics.Api.Shipments.Application.Errors;
using Logistics.Api.Shipments.Domain.Repositories;

namespace Logistics.Api.Shipments.Application.Queries.GetTrackingTimeline;

internal sealed class GetTrackingTimelineQueryHandler
    : IQueryHandler<GetTrackingTimelineQuery, Result<TrackingTimelineResponse>>
{
    private readonly IShipmentRepository _shipmentRepository;

    public GetTrackingTimelineQueryHandler(IShipmentRepository shipmentRepository)
    {
        _shipmentRepository = shipmentRepository;
    }

    public async Task<Result<TrackingTimelineResponse>> Handle(
        GetTrackingTimelineQuery query,
        CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByTrackingCodeAsync(query.TrackingCode, cancellationToken);
        if (shipment is null)
            return Result<TrackingTimelineResponse>.Failure(ShipmentErrors.NotFound);

        var events = shipment.TrackingEvents
            .OrderBy(x => x.OccurredAt)
            .Select(x => new TrackingTimelineEventResponse(
                EventId: x.Id,
                FromStatus: x.FromStatus?.ToString(),
                ToStatus: x.ToStatus.ToString(),
                HubCode: x.HubCode,
                Location: x.Location,
                Note: x.Note,
                OccurredAt: x.OccurredAt))
            .ToArray();

        return Result<TrackingTimelineResponse>.Success(new TrackingTimelineResponse(
            TrackingCode: shipment.TrackingCode,
            Events: Array.AsReadOnly(events)));
    }
}