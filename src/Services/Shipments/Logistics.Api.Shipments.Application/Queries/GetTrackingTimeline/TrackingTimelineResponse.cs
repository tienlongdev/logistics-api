namespace Logistics.Api.Shipments.Application.Queries.GetTrackingTimeline;

public sealed record TrackingTimelineEventResponse(
    Guid EventId,
    string? FromStatus,
    string ToStatus,
    string? HubCode,
    string? Location,
    string? Note,
    DateTimeOffset OccurredAt);

public sealed record TrackingTimelineResponse(
    string TrackingCode,
    IReadOnlyList<TrackingTimelineEventResponse> Events);