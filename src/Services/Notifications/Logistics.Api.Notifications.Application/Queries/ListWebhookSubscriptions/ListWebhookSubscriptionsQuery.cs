using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;
using Logistics.Api.Notifications.Application.Queries.GetWebhookSubscription;

namespace Logistics.Api.Notifications.Application.Queries.ListWebhookSubscriptions;

public sealed record ListWebhookSubscriptionsQuery(Guid RequestingUserId)
    : IQuery<Result<IReadOnlyList<WebhookSubscriptionResponse>>>;