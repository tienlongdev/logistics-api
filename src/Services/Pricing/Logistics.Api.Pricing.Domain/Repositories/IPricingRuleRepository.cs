using Logistics.Api.Pricing.Domain.Entities;
using Logistics.Api.Pricing.Domain.Enums;

namespace Logistics.Api.Pricing.Domain.Repositories;

/// <summary>
/// Repository contract for PricingRule persistence.
/// </summary>
public interface IPricingRuleRepository
{
    /// <summary>
    /// Returns all active rules for the given service type,
    /// ordered by Priority descending (highest first).
    /// </summary>
    Task<IReadOnlyList<PricingRule>> GetActiveRulesAsync(
        ServiceType serviceType,
        CancellationToken ct = default);

    void Add(PricingRule rule);

    Task SaveChangesAsync(CancellationToken ct = default);
}
