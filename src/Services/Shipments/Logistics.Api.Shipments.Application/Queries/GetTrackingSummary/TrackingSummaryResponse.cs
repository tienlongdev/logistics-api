namespace Logistics.Api.Shipments.Application.Queries.GetTrackingSummary;

public sealed record TrackingSummaryResponse(
    string TrackingCode,
    string ShipmentCode,
    string CurrentStatus,
    string ReceiverName,
    DateTimeOffset LastUpdatedAt);