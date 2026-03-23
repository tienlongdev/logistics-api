using Logistics.Api.BuildingBlocks.Application.Results;

namespace Logistics.Api.Pricing.Application.Services;

/// <summary>
/// Application-level service contract for fee calculation.
/// Callers receive a <see cref="Result{FeeBreakdown}"/>:
///   - Success → fee breakdown with amounts and the matched rule id.
///   - Failure → <c>pricing.no_rule_found</c> when no active rule matches.
///
/// Implementations query <see cref="Logistics.Api.Pricing.Domain.Repositories.IPricingRuleRepository"/>
/// and delegate fee math to the domain entity.
/// </summary>
public interface IPricingCalculator
{
    Task<Result<FeeBreakdown>> CalculateAsync(
        CalculateFeeRequest request,
        CancellationToken ct = default);
}
