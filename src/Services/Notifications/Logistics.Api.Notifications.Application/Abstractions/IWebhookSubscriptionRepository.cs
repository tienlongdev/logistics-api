namespace Logistics.Api.Notifications.Application.Abstractions;

public sealed record WebhookSubscriptionRecord(
    Guid Id,
    Guid MerchantId,
    string CallbackUrl,
    string[] Events,
    string Secret,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public interface IWebhookSubscriptionRepository
{
    Task<IReadOnlyList<WebhookSubscriptionRecord>> ListByMerchantAsync(Guid merchantId, CancellationToken ct = default);
    Task<WebhookSubscriptionRecord?> GetByIdAsync(Guid merchantId, Guid id, CancellationToken ct = default);
    Task<WebhookSubscriptionRecord> AddAsync(Guid merchantId, string callbackUrl, string[] events, string secret, CancellationToken ct = default);
    Task<WebhookSubscriptionRecord?> UpdateAsync(Guid merchantId, Guid id, string callbackUrl, string[] events, bool isActive, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid merchantId, Guid id, CancellationToken ct = default);
}