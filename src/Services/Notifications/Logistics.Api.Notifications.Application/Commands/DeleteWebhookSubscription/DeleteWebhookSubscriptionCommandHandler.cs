using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;
using Logistics.Api.Notifications.Application.Abstractions;
using Logistics.Api.Notifications.Application.Errors;

namespace Logistics.Api.Notifications.Application.Commands.DeleteWebhookSubscription;

internal sealed class DeleteWebhookSubscriptionCommandHandler : ICommandHandler<DeleteWebhookSubscriptionCommand, Result>
{
    private readonly IMerchantScopeService _merchantScopeService;
    private readonly IWebhookSubscriptionRepository _repository;

    public DeleteWebhookSubscriptionCommandHandler(
        IMerchantScopeService merchantScopeService,
        IWebhookSubscriptionRepository repository)
    {
        _merchantScopeService = merchantScopeService;
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteWebhookSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var merchant = await _merchantScopeService.GetByUserIdAsync(command.RequestingUserId, cancellationToken);
        if (merchant is null)
            return Result.Failure(NotificationErrors.MerchantScopeForbidden);

        var deleted = await _repository.DeleteAsync(merchant.MerchantId, command.SubscriptionId, cancellationToken);
        return deleted ? Result.Success() : Result.Failure(NotificationErrors.NotFound);
    }
}