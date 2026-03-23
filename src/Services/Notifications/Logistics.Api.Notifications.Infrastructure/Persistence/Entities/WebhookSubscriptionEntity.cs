namespace Logistics.Api.Notifications.Infrastructure.Persistence.Entities;

public sealed class WebhookSubscriptionEntity
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string CallbackUrl { get; set; } = default!;
    public string[] Events { get; set; } = [];
    public string Secret { get; set; } = default!;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}