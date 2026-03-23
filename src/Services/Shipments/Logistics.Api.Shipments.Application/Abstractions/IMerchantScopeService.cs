namespace Logistics.Api.Shipments.Application.Abstractions;

/// <summary>
/// Resolves merchant information for the currently authenticated user.
/// Implemented in Infrastructure via a cross-module read against
/// <c>merchants.merchant_users + merchants.merchants</c>.
/// </summary>
public interface IMerchantScopeService
{
    /// <summary>
    /// Returns the merchant the user belongs to, or <c>null</c> if the user
    /// has no merchant association or the merchant is inactive.
    /// </summary>
    Task<MerchantInfo?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}
