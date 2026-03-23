using System.Data;
using Dapper;
using Logistics.Api.Shipments.Application.Abstractions;
using Logistics.Api.Shipments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Api.Shipments.Infrastructure.Services;

/// <summary>
/// Resolves merchant information for a user by querying
/// <c>merchants.merchant_users</c> and <c>merchants.merchants</c> tables
/// via Dapper on the Shipments DbContext connection.
///
/// This is a legitimate cross-schema read in a Modular Monolith.
/// All schemas share the same PostgreSQL database instance.
/// </summary>
internal sealed class MerchantScopeService : IMerchantScopeService
{
    private readonly ShipmentsDbContext _context;
    private readonly ILogger<MerchantScopeService> _logger;

    public MerchantScopeService(ShipmentsDbContext context, ILogger<MerchantScopeService> logger)
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
            WHERE mu.user_id = @userId
              AND m.is_active = true
            LIMIT 1
            """;

        try
        {
            var conn = _context.Database.GetDbConnection();
            if (conn.State == ConnectionState.Closed)
                await ((System.Data.Common.DbConnection)conn).OpenAsync(ct);

            return await conn.QueryFirstOrDefaultAsync<MerchantInfo>(sql, new { userId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve merchant scope for user {UserId}", userId);
            return null;
        }
    }
}
