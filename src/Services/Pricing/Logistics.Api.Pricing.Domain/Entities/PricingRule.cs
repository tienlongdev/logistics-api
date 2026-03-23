using Logistics.Api.BuildingBlocks.Domain.Abstractions;
using Logistics.Api.Pricing.Domain.Enums;

namespace Logistics.Api.Pricing.Domain.Entities;

/// <summary>
/// Represents a matchable pricing rule.
///
/// Matching logic (in order):
///   1. ServiceType must match.
///   2. ZoneType must match the computed zone for the shipment.
///   3. WeightGram must fall in [MinWeightGram, MaxWeightGram) — MaxWeightGram == null means no upper bound.
///   4. Rule must be active (IsActive == true).
///   5. Current date must be within [EffectiveFrom, EffectiveTo] — EffectiveTo == null means no expiry.
///
/// When multiple rules match, the one with the highest Priority wins.
///
/// Fee formula:
///   shippingFee = BaseFee + max(0, ceil((weightGram - MinWeightGram) / 1000.0)) * PerKgFee
///   codFee      = CodAmount * (CodFeePercent / 100)
///   totalFee    = shippingFee + codFee
/// </summary>
public sealed class PricingRule : Entity<Guid>
{
    // Required by EF Core
    private PricingRule() { }

    public string Name { get; private set; } = default!;

    public ServiceType ServiceType { get; private set; }

    /// <summary>
    /// Zone type for this rule. Null means "matches any zone" (wildcard — useful for catch-all rules).
    /// </summary>
    public ZoneType? ZoneType { get; private set; }

    /// <summary>
    /// Optional province filter (origin). Null = any origin province.
    /// Used for highly-specific routes that override zone-level rules.
    /// </summary>
    public string? FromProvince { get; private set; }

    /// <summary>Optional province filter (destination). Null = any destination province.</summary>
    public string? ToProvince { get; private set; }

    /// <summary>Minimum weight (inclusive), in grams.</summary>
    public int MinWeightGram { get; private set; }

    /// <summary>Maximum weight (exclusive), in grams. Null = no upper bound.</summary>
    public int? MaxWeightGram { get; private set; }

    /// <summary>Fixed base fee in the system currency (VND).</summary>
    public decimal BaseFee { get; private set; }

    /// <summary>Additional fee per kg above the base bracket, in VND.</summary>
    public decimal PerKgFee { get; private set; }

    /// <summary>COD collection fee as a percentage of the COD amount (e.g. 2.5 = 2.5%).</summary>
    public decimal CodFeePercent { get; private set; }

    public bool IsActive { get; private set; }

    public DateOnly EffectiveFrom { get; private set; }

    /// <summary>Null = no expiry date (rule is valid indefinitely after EffectiveFrom).</summary>
    public DateOnly? EffectiveTo { get; private set; }

    /// <summary>Higher value wins when multiple rules match.</summary>
    public int Priority { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    // ── Factory ──────────────────────────────────────────────────────────────

    public static PricingRule Create(
        string name,
        ServiceType serviceType,
        ZoneType? zoneType,
        string? fromProvince,
        string? toProvince,
        int minWeightGram,
        int? maxWeightGram,
        decimal baseFee,
        decimal perKgFee,
        decimal codFeePercent,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        int priority,
        DateTimeOffset createdAt)
    {
        if (maxWeightGram.HasValue && maxWeightGram.Value <= minWeightGram)
            throw new ArgumentException("MaxWeightGram must be greater than MinWeightGram.");

        if (baseFee < 0) throw new ArgumentException("BaseFee cannot be negative.");
        if (perKgFee < 0) throw new ArgumentException("PerKgFee cannot be negative.");
        if (codFeePercent < 0 || codFeePercent > 100)
            throw new ArgumentException("CodFeePercent must be between 0 and 100.");

        return new PricingRule
        {
            Id = Guid.NewGuid(),
            Name = name,
            ServiceType = serviceType,
            ZoneType = zoneType,
            FromProvince = fromProvince,
            ToProvince = toProvince,
            MinWeightGram = minWeightGram,
            MaxWeightGram = maxWeightGram,
            BaseFee = baseFee,
            PerKgFee = perKgFee,
            CodFeePercent = codFeePercent,
            IsActive = true,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            Priority = priority,
            CreatedAt = createdAt
        };
    }

    // ── Domain behaviour ─────────────────────────────────────────────────────

    /// <summary>
    /// Returns true when this rule is applicable for the given inputs on a given date.
    /// </summary>
    public bool Matches(ServiceType serviceType, ZoneType zone, int weightGram, DateOnly on)
    {
        if (ServiceType != serviceType) return false;
        if (ZoneType.HasValue && ZoneType.Value != zone) return false;
        if (weightGram < MinWeightGram) return false;
        if (MaxWeightGram.HasValue && weightGram >= MaxWeightGram.Value) return false;
        if (on < EffectiveFrom) return false;
        if (EffectiveTo.HasValue && on > EffectiveTo.Value) return false;
        return IsActive;
    }

    /// <summary>
    /// Calculates the shipping fee (excluding COD) for a given weight.
    /// Extra weight is rounded up to the nearest kg above the bracket minimum.
    /// </summary>
    public decimal CalculateShippingFee(int weightGram)
    {
        var extraGrams = Math.Max(0, weightGram - MinWeightGram);
        var extraKg = (int)Math.Ceiling(extraGrams / 1000.0);
        return BaseFee + extraKg * PerKgFee;
    }

    /// <summary>Calculates the COD collection fee.</summary>
    public decimal CalculateCodFee(decimal codAmount)
        => Math.Round(codAmount * CodFeePercent / 100m, 2, MidpointRounding.AwayFromZero);
}
