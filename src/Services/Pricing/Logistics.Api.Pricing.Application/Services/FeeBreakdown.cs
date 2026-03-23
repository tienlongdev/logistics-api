using Logistics.Api.Pricing.Domain.Enums;

namespace Logistics.Api.Pricing.Application.Services;

/// <summary>
/// Fee breakdown returned by <see cref="IPricingCalculator"/>.
/// All amounts are in system currency (VND).
/// </summary>
public sealed record FeeBreakdown(
    decimal ShippingFee,
    decimal CodFee,
    decimal TotalFee,
    ZoneType ResolvedZone,
    Guid MatchedRuleId);
