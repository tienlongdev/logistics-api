using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;

namespace Logistics.Api.Notifications.Application.Commands.DeleteWebhookSubscription;

public sealed record DeleteWebhookSubscriptionCommand(
    Guid RequestingUserId,
    Guid SubscriptionId) : ICommand<Result>;