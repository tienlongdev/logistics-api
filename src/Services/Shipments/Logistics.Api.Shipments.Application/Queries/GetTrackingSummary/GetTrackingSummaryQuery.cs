using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;

namespace Logistics.Api.Shipments.Application.Queries.GetTrackingSummary;

public sealed record GetTrackingSummaryQuery(string TrackingCode)
    : IQuery<Result<TrackingSummaryResponse>>;