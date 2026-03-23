using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;
using Logistics.Api.Notifications.Application.Queries.GetWebhookSubscription;

namespace Logistics.Api.Notifications.Application.Commands.UpdateWebhookSubscription;

public sealed record UpdateWebhookSubscriptionCommand(
    Guid RequestingUserId,
    Guid SubscriptionId,
    string CallbackUrl,
    string[] Events,
    bool IsActive) : ICommand<Result<WebhookSubscriptionResponse>>;