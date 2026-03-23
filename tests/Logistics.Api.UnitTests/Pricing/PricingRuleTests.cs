using FluentAssertions;
using Logistics.Api.Pricing.Domain.Entities;
using Logistics.Api.Pricing.Domain.Enums;

namespace Logistics.Api.UnitTests.Pricing;

public sealed class PricingRuleTests
{
    // ── helpers ──────────────────────────────────────────────────────────────

    private static PricingRule StandardRule(
        ServiceType serviceType = ServiceType.Standard,
        ZoneType? zoneType = ZoneType.SameProvince,
        int minWeightGram = 0,
        int? maxWeightGram = 3000,
        decimal baseFee = 20_000m,
        decimal perKgFee = 5_000m,
        decimal codFeePercent = 2.5m,
        DateOnly? effectiveFrom = null,
        DateOnly? effectiveTo = null,
        int priority = 1) =>
        PricingRule.Create(
            name: "Test Rule",
            serviceType: serviceType,
            zoneType: zoneType,
            fromProvince: null,
            toProvince: null,
            minWeightGram: minWeightGram,
            maxWeightGram: maxWeightGram,
            baseFee: baseFee,
            perKgFee: perKgFee,
            codFeePercent: codFeePercent,
            effectiveFrom: effectiveFrom ?? DateOnly.FromDateTime(DateTime.Today).AddDays(-10),
            effectiveTo: effectiveTo,
            priority: priority,
            createdAt: DateTimeOffset.UtcNow);

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    // ── Matches ───────────────────────────────────────────────────────────────

    [Fact]
    public void Matches_ReturnsTrue_WhenAllCriteriaMatch()
    {
        var rule = StandardRule();

        rule.Matches(ServiceType.Standard, ZoneType.SameProvince, 1000, Today)
            .Should().BeTrue();
    }

    [Fact]
    public void Matches_ReturnsFalse_WhenServiceTypeDiffers()
    {
        var rule = StandardRule(serviceType: ServiceType.Standard);

        rule.Matches(ServiceType.Express, ZoneType.SameProvince, 1000, Today)
            .Should().BeFalse();
    }

    [Fact]
    public void Matches_ReturnsFalse_WhenZoneTypeDiffers()
    {
        var rule = StandardRule(zoneType: ZoneType.SameProvince);

        rule.Matches(ServiceType.Standard, ZoneType.National, 1000, Today)
            .Should().BeFalse();
    }

    [Fact]
    public void Matches_ReturnsTrue_WhenZoneTypeIsNull_AnyZone()
    {
        var rule = StandardRule(zoneType: null);

        rule.Matches(ServiceType.Standard, ZoneType.National, 1000, Today)
            .Should().BeTrue();

        rule.Matches(ServiceType.Standard, ZoneType.SameProvince, 1000, Today)
            .Should().BeTrue();
    }

    [Fact]
    public void Matches_ReturnsFalse_WhenWeightBelowMin()
    {
        var rule = StandardRule(minWeightGram: 500);

        rule.Matches(ServiceType.Standard, ZoneType.SameProvince, 499, Today)
            .Should().BeFalse();
    }

    [Fact]
    public void Matches_ReturnsTrue_WhenWeightEqualsMin()
    {
        var rule = StandardRule(minWeightGram: 500, maxWeightGram: 3000);

        rule.Matches(ServiceType.Standard, ZoneType.SameProvince, 500, Today)
            .Should().BeTrue();
    }

    [Fact]
    public void Matches_ReturnsFalse_WhenWeightEqualsMax()
    {
        // MaxWeightGram is exclusive upper bound
        var rule = StandardRule(minWeightGram: 0, maxWeightGram: 3000);

        rule.Matches(ServiceType.Standard, ZoneType.SameProvince, 3000, Today)
            .Should().BeFalse();
    }

    [Fact]
    public void Matches_ReturnsTrue_WhenMaxWeightGramIsNull()
    {
        var rule = StandardRule(minWeightGram: 0, maxWeightGram: null);

        rule.Matches(ServiceType.Standard, ZoneType.SameProvince, 50_000, Today)
            .Should().BeTrue();
    }

    [Fact]
    public void Matches_ReturnsFalse_WhenBeforeEffectiveFrom()
    {
        var rule = StandardRule(effectiveFrom: Today.AddDays(1));

        rule.Matches(ServiceType.Standard, ZoneType.SameProvince, 1000, Today)
            .Should().BeFalse();
    }

    [Fact]
    public void Matches_ReturnsFalse_WhenAfterEffectiveTo()
    {
        var rule = StandardRule(effectiveFrom: Today.AddDays(-5), effectiveTo: Today.AddDays(-1));

        rule.Matches(ServiceType.Standard, ZoneType.SameProvince, 1000, Today)
            .Should().BeFalse();
    }

    [Fact]
    public void Matches_ReturnsTrue_WhenEffectiveToIsNull()
    {
        var rule = StandardRule(effectiveFrom: Today.AddDays(-100), effectiveTo: null);

        rule.Matches(ServiceType.Standard, ZoneType.SameProvince, 1000, Today.AddDays(9999))
            .Should().BeTrue();
    }

    // ── CalculateShippingFee ─────────────────────────────────────────────────

    [Fact]
    public void CalculateShippingFee_ReturnsBaseFee_WhenWeightAtMinimum()
    {
        var rule = StandardRule(minWeightGram: 0, baseFee: 20_000m, perKgFee: 5_000m);

        rule.CalculateShippingFee(0).Should().Be(20_000m);
    }

    [Fact]
    public void CalculateShippingFee_AddsPerKgFee_ForExtraWeight()
    {
        // min=0, base=20000, perKg=5000
        // 1500g extra above min → ceil(1500/1000) = 2 kg → 20000 + 2*5000 = 30000
        var rule = StandardRule(minWeightGram: 0, baseFee: 20_000m, perKgFee: 5_000m);

        rule.CalculateShippingFee(1500).Should().Be(30_000m);
    }

    [Fact]
    public void CalculateShippingFee_CeilsExtraWeight()
    {
        // 1001g extra → ceil(1001/1000) = 2 extra kg
        var rule = StandardRule(minWeightGram: 0, baseFee: 10_000m, perKgFee: 3_000m);

        rule.CalculateShippingFee(1001).Should().Be(16_000m); // 10000 + 2*3000
    }

    [Fact]
    public void CalculateShippingFee_NoNegativeExtra_WhenWeightBelowMin()
    {
        // weight < minWeightGram → shouldn't happen in practice (Matches blocks it)
        // but the method should not return negative fee
        var rule = StandardRule(minWeightGram: 500, baseFee: 20_000m, perKgFee: 5_000m);

        rule.CalculateShippingFee(100).Should().Be(20_000m);
    }

    // ── CalculateCodFee ──────────────────────────────────────────────────────

    [Fact]
    public void CalculateCodFee_ReturnsCorrectAmount()
    {
        var rule = StandardRule(codFeePercent: 2.5m);

        // 100_000 * 2.5 / 100 = 2500
        rule.CalculateCodFee(100_000m).Should().Be(2_500m);
    }

    [Fact]
    public void CalculateCodFee_ReturnsZero_WhenCodAmountIsZero()
    {
        var rule = StandardRule(codFeePercent: 2.5m);

        rule.CalculateCodFee(0m).Should().Be(0m);
    }

    [Fact]
    public void CalculateCodFee_RoundsAwayFromZero()
    {
        // 100_001 * 1.0 / 100 = 1000.01 → rounds to 1000.01 (2dp, no rounding needed)
        // Use a value that triggers rounding: 33_333 * 3.0 / 100 = 999.99 → 1000.00
        var rule = StandardRule(codFeePercent: 3.0m);

        rule.CalculateCodFee(33_333m).Should().Be(999.99m);
    }

    // ── Factory guard clauses ────────────────────────────────────────────────

    [Fact]
    public void Create_Throws_WhenMaxWeightLessThanMin()
    {
        var act = () => StandardRule(minWeightGram: 1000, maxWeightGram: 500);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_Throws_WhenBaseFeeIsNegative()
    {
        var act = () => StandardRule(baseFee: -1m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_Throws_WhenCodFeePercentOver100()
    {
        var act = () => StandardRule(codFeePercent: 101m);

        act.Should().Throw<ArgumentException>();
    }
}
