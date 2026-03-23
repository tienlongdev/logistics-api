namespace Logistics.Api.Notifications.Application.Queries.GetWebhookSubscription;

public sealed record WebhookSubscriptionResponse(
    Guid Id,
    string CallbackUrl,
    IReadOnlyList<string> Events,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);