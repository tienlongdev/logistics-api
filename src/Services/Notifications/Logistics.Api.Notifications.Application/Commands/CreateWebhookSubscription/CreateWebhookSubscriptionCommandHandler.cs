using System.Security.Cryptography;
using Logistics.Api.BuildingBlocks.Application.Abstractions.CQRS;
using Logistics.Api.BuildingBlocks.Application.Results;
using Logistics.Api.Notifications.Application.Abstractions;
using Logistics.Api.Notifications.Application.Errors;

namespace Logistics.Api.Notifications.Application.Commands.CreateWebhookSubscription;

internal sealed class CreateWebhookSubscriptionCommandHandler
    : ICommandHandler<CreateWebhookSubscriptionCommand, Result<CreateWebhookSubscriptionResponse>>
{
    private readonly IMerchantScopeService _merchantScopeService;
    private readonly IWebhookSubscriptionRepository _repository;

    public CreateWebhookSubscriptionCommandHandler(
        IMerchantScopeService merchantScopeService,
        IWebhookSubscriptionRepository repository)
    {
        _merchantScopeService = merchantScopeService;
        _repository = repository;
    }

    public async Task<Result<CreateWebhookSubscriptionResponse>> Handle(
        CreateWebhookSubscriptionCommand command,
        CancellationToken cancellationToken)
    {
        var merchant = await _merchantScopeService.GetByUserIdAsync(command.RequestingUserId, cancellationToken);
        if (merchant is null)
            return Result<CreateWebhookSubscriptionResponse>.Failure(NotificationErrors.MerchantScopeForbidden);

        var signingSecret = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        var created = await _repository.AddAsync(
            merchant.MerchantId,
            command.CallbackUrl,
            command.Events.Distinct(StringComparer.Ordinal).ToArray(),
            signingSecret,
            cancellationToken);

        return Result<CreateWebhookSubscriptionResponse>.Success(new CreateWebhookSubscriptionResponse(
            created.Id,
            created.CallbackUrl,
            created.Events,
            created.IsActive,
            signingSecret,
            created.CreatedAt,
            created.UpdatedAt));
    }
}