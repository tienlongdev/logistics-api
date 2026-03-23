using Logistics.Api.Notifications.Application.Abstractions;
using Logistics.Api.Notifications.Infrastructure.Persistence;
using Logistics.Api.Notifications.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api.Notifications.Infrastructure.Repositories;

internal sealed class WebhookSubscriptionRepository : IWebhookSubscriptionRepository
{
    private readonly NotificationsDbContext _context;

    public WebhookSubscriptionRepository(NotificationsDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<WebhookSubscriptionRecord>> ListByMerchantAsync(Guid merchantId, CancellationToken ct = default)
    {
        var items = await _context.WebhookSubscriptions
            .Where(x => x.MerchantId == merchantId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        return items.Select(Map).ToArray();
    }

    public async Task<WebhookSubscriptionRecord?> GetByIdAsync(Guid merchantId, Guid id, CancellationToken ct = default)
    {
        var entity = await _context.WebhookSubscriptions
            .FirstOrDefaultAsync(x => x.MerchantId == merchantId && x.Id == id, ct);

        return entity is null ? null : Map(entity);
    }

    public async Task<WebhookSubscriptionRecord> AddAsync(Guid merchantId, string callbackUrl, string[] events, string secret, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new WebhookSubscriptionEntity
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            CallbackUrl = callbackUrl,
            Events = events,
            Secret = secret,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.WebhookSubscriptions.Add(entity);
        await _context.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<WebhookSubscriptionRecord?> UpdateAsync(Guid merchantId, Guid id, string callbackUrl, string[] events, bool isActive, CancellationToken ct = default)
    {
        var entity = await _context.WebhookSubscriptions
            .FirstOrDefaultAsync(x => x.MerchantId == merchantId && x.Id == id, ct);

        if (entity is null)
            return null;

        entity.CallbackUrl = callbackUrl;
        entity.Events = events;
        entity.IsActive = isActive;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(Guid merchantId, Guid id, CancellationToken ct = default)
    {
        var entity = await _context.WebhookSubscriptions
            .FirstOrDefaultAsync(x => x.MerchantId == merchantId && x.Id == id, ct);

        if (entity is null)
            return false;

        _context.WebhookSubscriptions.Remove(entity);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    private static WebhookSubscriptionRecord Map(WebhookSubscriptionEntity entity) =>
        new(entity.Id, entity.MerchantId, entity.CallbackUrl, entity.Events, entity.Secret, entity.IsActive, entity.CreatedAt, entity.UpdatedAt);
}