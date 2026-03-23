using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;
using Logistics.Api.Notifications.Application.Abstractions;
using Logistics.Api.Notifications.Application.Errors;
using Logistics.Api.Notifications.Application.Queries.GetWebhookSubscription;

namespace Logistics.Api.Notifications.Application.Queries.ListWebhookSubscriptions;

internal sealed class ListWebhookSubscriptionsQueryHandler
    : IQueryHandler<ListWebhookSubscriptionsQuery, Result<IReadOnlyList<WebhookSubscriptionResponse>>>
{
    private readonly IMerchantScopeService _merchantScopeService;
    private readonly IWebhookSubscriptionRepository _repository;

    public ListWebhookSubscriptionsQueryHandler(
        IMerchantScopeService merchantScopeService,
        IWebhookSubscriptionRepository repository)
    {
        _merchantScopeService = merchantScopeService;
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<WebhookSubscriptionResponse>>> Handle(
        ListWebhookSubscriptionsQuery query,
        CancellationToken cancellationToken)
    {
        var merchant = await _merchantScopeService.GetByUserIdAsync(query.RequestingUserId, cancellationToken);
        if (merchant is null)
            return Result<IReadOnlyList<WebhookSubscriptionResponse>>.Failure(NotificationErrors.MerchantScopeForbidden);

        var subscriptions = await _repository.ListByMerchantAsync(merchant.MerchantId, cancellationToken);
        var items = subscriptions
            .Select(x => new WebhookSubscriptionResponse(
                x.Id,
                x.CallbackUrl,
                x.Events,
                x.IsActive,
                x.CreatedAt,
                x.UpdatedAt))
            .ToArray();

        return Result<IReadOnlyList<WebhookSubscriptionResponse>>.Success(Array.AsReadOnly(items));
    }
}