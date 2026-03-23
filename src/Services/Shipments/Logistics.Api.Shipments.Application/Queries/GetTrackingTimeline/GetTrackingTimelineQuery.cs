using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;

namespace Logistics.Api.Shipments.Application.Queries.GetTrackingTimeline;

public sealed record GetTrackingTimelineQuery(string TrackingCode)
    : IQuery<Result<TrackingTimelineResponse>>;