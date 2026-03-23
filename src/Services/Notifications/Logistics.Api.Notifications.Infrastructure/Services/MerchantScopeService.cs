using Dapper;
using Logistics.Api.Notifications.Application.Abstractions;
using Logistics.Api.Notifications.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Api.Notifications.Infrastructure.Services;

internal sealed class MerchantScopeService : IMerchantScopeService
{
    private readonly NotificationsDbContext _context;
    private readonly ILogger<MerchantScopeService> _logger;

    public MerchantScopeService(NotificationsDbContext context, ILogger<MerchantScopeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MerchantInfo?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT m.id AS MerchantId, m.code AS MerchantCode, m.name AS MerchantName
            FROM merchants.merchants m
            INNER JOIN merchants.merchant_users mu ON mu.merchant_id = m.id
            WHERE mu.user_id = @userId AND m.is_active = true
            LIMIT 1
            """;

        try
        {
            var conn = _context.Database.GetDbConnection();
            return await conn.QueryFirstOrDefaultAsync<MerchantInfo>(sql, new { userId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve merchant scope for user {UserId}", userId);
            return null;
        }
    }
}