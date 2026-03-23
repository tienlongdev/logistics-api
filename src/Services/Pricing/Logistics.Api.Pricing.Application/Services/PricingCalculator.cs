using Logistics.Api.BuildingBlocks.Application.Results;
using Logistics.Api.BuildingBlocks.Domain.Time;
using Logistics.Api.Pricing.Application.Errors;
using Logistics.Api.Pricing.Domain.Repositories;
using Logistics.Api.Pricing.Domain.Services;
using Microsoft.Extensions.Logging;

namespace Logistics.Api.Pricing.Application.Services;

/// <summary>
/// Concrete implementation of <see cref="IPricingCalculator"/>.
///
/// Algorithm:
///   1. Resolve zone from sender/receiver provinces (via <see cref="ZoneResolver"/>).
///   2. Load active rules for the requested service type from the repository.
///   3. Filter to rules that match (ZoneType, weight bracket, effective date).
///   4. Pick the rule with the highest Priority.
///   5. Delegate fee maths to the domain entity methods.
/// </summary>
internal sealed class PricingCalculator : IPricingCalculator
{
    private readonly IPricingRuleRepository _repository;
    private readonly IClock _clock;
    private readonly ILogger<PricingCalculator> _logger;

    public PricingCalculator(
        IPricingRuleRepository repository,
        IClock clock,
        ILogger<PricingCalculator> logger)
    {
        _repository = repository;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result<FeeBreakdown>> CalculateAsync(
        CalculateFeeRequest request,
        CancellationToken ct = default)
    {
        var zone = ZoneResolver.Resolve(request.SenderProvince, request.ReceiverProvince);
        var today = DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime);

        var rules = await _repository.GetActiveRulesAsync(request.ServiceType, ct);

        var matchedRule = rules
            .Where(r => r.Matches(request.ServiceType, zone, request.WeightGram, today))
            .OrderByDescending(r => r.Priority)
            .FirstOrDefault();

        if (matchedRule is null)
        {
            _logger.LogWarning(
                "No pricing rule found. ServiceType={ServiceType} Zone={Zone} Weight={Weight}g",
                request.ServiceType, zone, request.WeightGram);

            return Result<FeeBreakdown>.Failure(PricingErrors.NoPricingRuleFound);
        }

        var shippingFee = matchedRule.CalculateShippingFee(request.WeightGram);
        var codFee = matchedRule.CalculateCodFee(request.CodAmount);
        var totalFee = shippingFee + codFee;

        _logger.LogDebug(
            "Pricing: Rule={RuleId} Zone={Zone} ShippingFee={ShippingFee} CodFee={CodFee} Total={Total}",
            matchedRule.Id, zone, shippingFee, codFee, totalFee);

        return Result<FeeBreakdown>.Success(
            new FeeBreakdown(shippingFee, codFee, totalFee, zone, matchedRule.Id));
    }
}
