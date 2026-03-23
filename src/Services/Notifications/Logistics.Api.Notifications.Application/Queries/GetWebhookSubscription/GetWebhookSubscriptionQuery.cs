using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;

namespace Logistics.Api.Notifications.Application.Queries.GetWebhookSubscription;

public sealed record GetWebhookSubscriptionQuery(
    Guid RequestingUserId,
    Guid SubscriptionId) : IQuery<Result<WebhookSubscriptionResponse>>;