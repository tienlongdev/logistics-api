namespace Logistics.Api.Notifications.Application.Commands.CreateWebhookSubscription;

public sealed record CreateWebhookSubscriptionResponse(
    Guid Id,
    string CallbackUrl,
    IReadOnlyList<string> Events,
    bool IsActive,
    string SigningSecret,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);