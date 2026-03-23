using System.Security.Claims;
using Asp.Versioning;
using Logistics.Api.Host.Extensions;
using Logistics.Api.Notifications.Application.Commands.CreateWebhookSubscription;
using Logistics.Api.Notifications.Application.Commands.DeleteWebhookSubscription;
using Logistics.Api.Notifications.Application.Commands.UpdateWebhookSubscription;
using Logistics.Api.Notifications.Application.Queries.GetWebhookSubscription;
using Logistics.Api.Notifications.Application.Queries.ListWebhookSubscriptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.Api.Host.Controllers.Webhooks;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/webhooks/subscriptions")]
[Authorize(Policy = "MerchantOnly")]
public sealed class WebhookSubscriptionsController : ControllerBase
{
    private readonly ISender _sender;

    public WebhookSubscriptionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WebhookSubscriptionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var userId = TryGetUserId(User);
        if (!userId.HasValue)
            return Unauthorized();

        var result = await _sender.Send(new ListWebhookSubscriptionsQuery(userId.Value), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemResult(HttpContext);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WebhookSubscriptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var userId = TryGetUserId(User);
        if (!userId.HasValue)
            return Unauthorized();

        var result = await _sender.Send(new GetWebhookSubscriptionQuery(userId.Value, id), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemResult(HttpContext);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateWebhookSubscriptionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateWebhookSubscriptionHttpRequest request, CancellationToken ct)
    {
        var userId = TryGetUserId(User);
        if (!userId.HasValue)
            return Unauthorized();

        var result = await _sender.Send(
            new CreateWebhookSubscriptionCommand(userId.Value, request.CallbackUrl, request.Events),
            ct);

        if (result.IsFailure)
            return result.ToProblemResult(HttpContext);

        return CreatedAtAction(nameof(Get), new { id = result.Value.Id, version = "1.0" }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(WebhookSubscriptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWebhookSubscriptionHttpRequest request, CancellationToken ct)
    {
        var userId = TryGetUserId(User);
        if (!userId.HasValue)
            return Unauthorized();

        var result = await _sender.Send(
            new UpdateWebhookSubscriptionCommand(userId.Value, id, request.CallbackUrl, request.Events, request.IsActive),
            ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemResult(HttpContext);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = TryGetUserId(User);
        if (!userId.HasValue)
            return Unauthorized();

        var result = await _sender.Send(new DeleteWebhookSubscriptionCommand(userId.Value, id), ct);
        return result.IsSuccess ? NoContent() : result.ToProblemResult(HttpContext);
    }

    private static Guid? TryGetUserId(ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(raw, out var parsed) ? parsed : null;
    }
}

public sealed record CreateWebhookSubscriptionHttpRequest(string CallbackUrl, string[] Events);

public sealed record UpdateWebhookSubscriptionHttpRequest(string CallbackUrl, string[] Events, bool IsActive);