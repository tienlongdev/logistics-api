using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;

namespace Logistics.Api.Notifications.Application.Commands.CreateWebhookSubscription;

public sealed record CreateWebhookSubscriptionCommand(
    Guid RequestingUserId,
    string CallbackUrl,
    string[] Events) : ICommand<Result<CreateWebhookSubscriptionResponse>>;