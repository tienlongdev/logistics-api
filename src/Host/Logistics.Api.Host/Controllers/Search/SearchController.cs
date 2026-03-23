using System.Security.Claims;
using Asp.Versioning;
using Logistics.Api.Host.Extensions;
using Logistics.Api.Identity.Domain.Entities;
using Logistics.Api.Search.Application.Queries.SearchShipments;
using Logistics.Api.Shipments.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.Api.Host.Controllers.Search;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/search")]
[Authorize]
public sealed class SearchController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMerchantScopeService _merchantScopeService;

    public SearchController(ISender sender, IMerchantScopeService merchantScopeService)
    {
        _sender = sender;
        _merchantScopeService = merchantScopeService;
    }

    [HttpGet("shipments")]
    [ProducesResponseType(typeof(SearchShipmentsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SearchShipments([FromQuery] SearchShipmentsHttpRequest request, CancellationToken ct)
    {
        var forcedMerchantCode = await ResolveMerchantCodeFilterAsync(User, ct);
        if (User.IsInRole(Role.Names.Merchant) && forcedMerchantCode is null)
            return Forbid();

        var result = await _sender.Send(new SearchShipmentsQuery(
            request.TrackingCode,
            request.ShipmentCode,
            request.ReceiverPhone,
            forcedMerchantCode ?? request.MerchantCode,
            request.Status,
            request.FromDate,
            request.ToDate,
            request.Page ?? 1,
            request.PageSize ?? 20,
            request.Sort), ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemResult(HttpContext);
    }

    private async Task<string?> ResolveMerchantCodeFilterAsync(ClaimsPrincipal user, CancellationToken ct)
    {
        if (!user.IsInRole(Role.Names.Merchant))
            return null;

        var raw = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        if (!Guid.TryParse(raw, out var userId))
            return null;

        var merchant = await _merchantScopeService.GetByUserIdAsync(userId, ct);
        return merchant?.MerchantCode;
    }
}

public sealed record SearchShipmentsHttpRequest(
    string? TrackingCode,
    string? ShipmentCode,
    string? ReceiverPhone,
    string? MerchantCode,
    string? Status,
    DateTimeOffset? FromDate,
    DateTimeOffset? ToDate,
    int? Page,
    int? PageSize,
    string? Sort);