using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;
using Logistics.Api.Notifications.Application.Abstractions;
using Logistics.Api.Notifications.Application.Errors;
using Logistics.Api.Notifications.Application.Queries.GetWebhookSubscription;

namespace Logistics.Api.Notifications.Application.Commands.UpdateWebhookSubscription;

internal sealed class UpdateWebhookSubscriptionCommandHandler
    : ICommandHandler<UpdateWebhookSubscriptionCommand, Result<WebhookSubscriptionResponse>>
{
    private readonly IMerchantScopeService _merchantScopeService;
    private readonly IWebhookSubscriptionRepository _repository;

    public UpdateWebhookSubscriptionCommandHandler(
        IMerchantScopeService merchantScopeService,
        IWebhookSubscriptionRepository repository)
    {
        _merchantScopeService = merchantScopeService;
        _repository = repository;
    }

    public async Task<Result<WebhookSubscriptionResponse>> Handle(
        UpdateWebhookSubscriptionCommand command,
        CancellationToken cancellationToken)
    {
        var merchant = await _merchantScopeService.GetByUserIdAsync(command.RequestingUserId, cancellationToken);
        if (merchant is null)
            return Result<WebhookSubscriptionResponse>.Failure(NotificationErrors.MerchantScopeForbidden);

        var updated = await _repository.UpdateAsync(
            merchant.MerchantId,
            command.SubscriptionId,
            command.CallbackUrl,
            command.Events.Distinct(StringComparer.Ordinal).ToArray(),
            command.IsActive,
            cancellationToken);

        if (updated is null)
            return Result<WebhookSubscriptionResponse>.Failure(NotificationErrors.NotFound);

        return Result<WebhookSubscriptionResponse>.Success(new WebhookSubscriptionResponse(
            updated.Id,
            updated.CallbackUrl,
            updated.Events,
            updated.IsActive,
            updated.CreatedAt,
            updated.UpdatedAt));
    }
}