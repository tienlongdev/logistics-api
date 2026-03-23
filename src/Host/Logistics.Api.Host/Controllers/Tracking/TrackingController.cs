using Asp.Versioning;
using Logistics.Api.Host.Extensions;
using Logistics.Api.Shipments.Application.Queries.GetTrackingSummary;
using Logistics.Api.Shipments.Application.Queries.GetTrackingTimeline;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.Api.Host.Controllers.Tracking;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tracking")]
[AllowAnonymous]
public sealed class TrackingController : ControllerBase
{
    private readonly ISender _sender;

    public TrackingController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{trackingCode}")]
    [ProducesResponseType(typeof(TrackingSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrackingSummary(string trackingCode, CancellationToken ct)
    {
        var result = await _sender.Send(new GetTrackingSummaryQuery(trackingCode), ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemResult(HttpContext);
    }

    [HttpGet("{trackingCode}/timeline")]
    [ProducesResponseType(typeof(TrackingTimelineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrackingTimeline(string trackingCode, CancellationToken ct)
    {
        var result = await _sender.Send(new GetTrackingTimelineQuery(trackingCode), ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemResult(HttpContext);
    }
}