using Logistics.Api.Shipments.Application.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Logistics.Api.Shipments.Infrastructure.Idempotency;

/// <summary>
/// Redis-backed idempotency store (A4).
/// Uses <see cref="IDistributedCache"/> so the implementation is swappable
/// (Redis in production, in-memory in tests).
///
/// Key format: <c>idempotency:shipments:{key}</c>
/// TTL: 24 hours
/// Fails open — cache errors are logged and the request falls through to the DB.
/// </summary>
internal sealed class RedisIdempotencyService : IIdempotencyService
{
    private const string Prefix = "idempotency:shipments:";
    private static readonly TimeSpan Ttl = TimeSpan.FromHours(24);

    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisIdempotencyService> _logger;

    public RedisIdempotencyService(IDistributedCache cache, ILogger<RedisIdempotencyService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<Guid?> TryGetAsync(string key, CancellationToken ct = default)
    {
        try
        {
            var raw = await _cache.GetStringAsync(CacheKey(key), ct);
            if (raw is not null && Guid.TryParse(raw, out var id))
                return id;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Idempotency cache GET failed for key {Key}; falling back to DB", key);
        }
        return null;
    }

    public async Task StoreAsync(string key, Guid shipmentId, CancellationToken ct = default)
    {
        try
        {
            await _cache.SetStringAsync(
                CacheKey(key),
                shipmentId.ToString(),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = Ttl },
                ct);
        }
        catch (Exception ex)
        {
            // Non-fatal: the DB unique constraint ensures correctness even without cache
            _logger.LogWarning(ex, "Idempotency cache SET failed for key {Key}", key);
        }
    }

    private static string CacheKey(string key) => $"{Prefix}{key}";
}
