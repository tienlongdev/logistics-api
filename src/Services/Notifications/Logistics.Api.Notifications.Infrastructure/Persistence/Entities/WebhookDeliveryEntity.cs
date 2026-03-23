namespace Logistics.Api.Notifications.Infrastructure.Persistence.Entities;

public sealed class WebhookDeliveryEntity
{
    public Guid Id { get; set; }
    public Guid SubscriptionId { get; set; }
    public Guid MerchantId { get; set; }
    public string EventType { get; set; } = default!;
    public Guid EventId { get; set; }
    public string Payload { get; set; } = default!;
    public string Status { get; set; } = default!;
    public int AttemptCount { get; set; }
    public int MaxAttempts { get; set; }
    public DateTimeOffset? NextRetryAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    public int? LastResponseCode { get; set; }
    public string? LastResponseBody { get; set; }
    public string? LastError { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public WebhookSubscriptionEntity Subscription { get; set; } = default!;
}