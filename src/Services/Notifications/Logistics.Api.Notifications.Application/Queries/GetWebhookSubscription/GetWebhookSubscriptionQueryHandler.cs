using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;
using Logistics.Api.Notifications.Application.Abstractions;
using Logistics.Api.Notifications.Application.Errors;

namespace Logistics.Api.Notifications.Application.Queries.GetWebhookSubscription;

internal sealed class GetWebhookSubscriptionQueryHandler
    : IQueryHandler<GetWebhookSubscriptionQuery, Result<WebhookSubscriptionResponse>>
{
    private readonly IMerchantScopeService _merchantScopeService;
    private readonly IWebhookSubscriptionRepository _repository;

    public GetWebhookSubscriptionQueryHandler(
        IMerchantScopeService merchantScopeService,
        IWebhookSubscriptionRepository repository)
    {
        _merchantScopeService = merchantScopeService;
        _repository = repository;
    }

    public async Task<Result<WebhookSubscriptionResponse>> Handle(
        GetWebhookSubscriptionQuery query,
        CancellationToken cancellationToken)
    {
        var merchant = await _merchantScopeService.GetByUserIdAsync(query.RequestingUserId, cancellationToken);
        if (merchant is null)
            return Result<WebhookSubscriptionResponse>.Failure(NotificationErrors.MerchantScopeForbidden);

        var subscription = await _repository.GetByIdAsync(merchant.MerchantId, query.SubscriptionId, cancellationToken);
        if (subscription is null)
            return Result<WebhookSubscriptionResponse>.Failure(NotificationErrors.NotFound);

        return Result<WebhookSubscriptionResponse>.Success(new WebhookSubscriptionResponse(
            subscription.Id,
            subscription.CallbackUrl,
            subscription.Events,
            subscription.IsActive,
            subscription.CreatedAt,
            subscription.UpdatedAt));
    }
}