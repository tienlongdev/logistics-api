namespace Logistics.Api.Shipments.Application.Commands.TransitionShipmentStatus;

public sealed record TransitionShipmentStatusResponse(
    Guid ShipmentId,
    string TrackingCode,
    string FromStatus,
    string ToStatus,
    DateTimeOffset OccurredAt,
    string CurrentStatus);