using FluentAssertions;
using Logistics.Api.BuildingBlocks.Domain.Time;
using Logistics.Api.Pricing.Application.Errors;
using Logistics.Api.Pricing.Application.Services;
using Logistics.Api.Pricing.Domain.Entities;
using Logistics.Api.Pricing.Domain.Enums;
using Logistics.Api.Pricing.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Logistics.Api.UnitTests.Pricing;

public sealed class PricingCalculatorTests
{
    private readonly IPricingRuleRepository _repository = Substitute.For<IPricingRuleRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly PricingCalculator _sut;

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    public PricingCalculatorTests()
    {
        _clock.UtcNow.Returns(DateTimeOffset.UtcNow);
        _sut = new PricingCalculator(
            _repository,
            _clock,
            NullLogger<PricingCalculator>.Instance);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static PricingRule MakeRule(
        ServiceType serviceType = ServiceType.Standard,
        ZoneType? zoneType = ZoneType.SameProvince,
        int minWeightGram = 0,
        int? maxWeightGram = null,
        decimal baseFee = 20_000m,
        decimal perKgFee = 5_000m,
        decimal codFeePercent = 2.5m,
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
            effectiveFrom: Today.AddDays(-10),
            effectiveTo: null,
            priority: priority,
            createdAt: DateTimeOffset.UtcNow);

    // ── no matching rule ──────────────────────────────────────────────────────

    [Fact]
    public async Task CalculateAsync_ReturnsFailure_WhenNoRulesExist()
    {
        _repository.GetActiveRulesAsync(ServiceType.Standard, default)
            .Returns([]);

        var result = await _sut.CalculateAsync(
            new CalculateFeeRequest(ServiceType.Standard, "Hanoi", "Hanoi", 1000, 0m));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(PricingErrors.NoPricingRuleFound.Code);
    }

    [Fact]
    public async Task CalculateAsync_ReturnsFailure_WhenNoRuleMatches()
    {
        // Rule only covers Express; request is Standard
        var expressRule = MakeRule(serviceType: ServiceType.Express);
        _repository.GetActiveRulesAsync(ServiceType.Standard, default)
            .Returns([]);

        var result = await _sut.CalculateAsync(
            new CalculateFeeRequest(ServiceType.Standard, "Hanoi", "Hanoi", 1000, 0m));

        result.IsFailure.Should().BeTrue();
    }

    // ── successful calculation ────────────────────────────────────────────────

    [Fact]
    public async Task CalculateAsync_ReturnsCorrectFees_ForSameProvinceShipment()
    {
        // base=20000, perKg=5000, cod=2.5%; 1000g → 1 extra kg above min=0
        var rule = MakeRule(zoneType: ZoneType.SameProvince, baseFee: 20_000m, perKgFee: 5_000m, codFeePercent: 2.5m);
        _repository.GetActiveRulesAsync(ServiceType.Standard, default)
            .Returns([rule]);

        var result = await _sut.CalculateAsync(
            new CalculateFeeRequest(ServiceType.Standard, "Hanoi", "Hanoi", 1000, 100_000m));

        result.IsSuccess.Should().BeTrue();
        result.Value.ResolvedZone.Should().Be(ZoneType.SameProvince);
        result.Value.ShippingFee.Should().Be(25_000m);  // 20000 + 1*5000
        result.Value.CodFee.Should().Be(2_500m);         // 100000 * 2.5%
        result.Value.TotalFee.Should().Be(27_500m);
        result.Value.MatchedRuleId.Should().Be(rule.Id);
    }

    [Fact]
    public async Task CalculateAsync_ReturnsCorrectFees_ForNationalShipment()
    {
        var rule = MakeRule(zoneType: ZoneType.National, baseFee: 35_000m, perKgFee: 7_000m, codFeePercent: 3.0m);
        _repository.GetActiveRulesAsync(ServiceType.Standard, default)
            .Returns([rule]);

        var result = await _sut.CalculateAsync(
            new CalculateFeeRequest(ServiceType.Standard, "Hanoi", "Ho Chi Minh", 2000, 200_000m));

        result.IsSuccess.Should().BeTrue();
        result.Value.ResolvedZone.Should().Be(ZoneType.National);
        result.Value.ShippingFee.Should().Be(49_000m);  // 35000 + 2*7000
        result.Value.CodFee.Should().Be(6_000m);         // 200000 * 3%
        result.Value.TotalFee.Should().Be(55_000m);
    }

    // ── priority selection ────────────────────────────────────────────────────

    [Fact]
    public async Task CalculateAsync_PicksHighestPriorityRule_WhenMultipleMatch()
    {
        var lowPriority = MakeRule(priority: 1, baseFee: 20_000m);
        var highPriority = MakeRule(priority: 10, baseFee: 99_000m);

        // Repository already returns ordered by priority desc
        _repository.GetActiveRulesAsync(ServiceType.Standard, default)
            .Returns([highPriority, lowPriority]);

        var result = await _sut.CalculateAsync(
            new CalculateFeeRequest(ServiceType.Standard, "Hanoi", "Hanoi", 0, 0m));

        result.IsSuccess.Should().BeTrue();
        result.Value.MatchedRuleId.Should().Be(highPriority.Id);
        result.Value.ShippingFee.Should().Be(99_000m);
    }

    // ── zone resolution ───────────────────────────────────────────────────────

    [Fact]
    public async Task CalculateAsync_ResolvesNationalZone_WhenProvincesNull()
    {
        var rule = MakeRule(zoneType: ZoneType.National);
        _repository.GetActiveRulesAsync(ServiceType.Standard, default)
            .Returns([rule]);

        var result = await _sut.CalculateAsync(
            new CalculateFeeRequest(ServiceType.Standard, null, null, 500, 0m));

        result.IsSuccess.Should().BeTrue();
        result.Value.ResolvedZone.Should().Be(ZoneType.National);
    }

    // ── COD zero ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task CalculateAsync_CodFeeIsZero_WhenCodAmountIsZero()
    {
        var rule = MakeRule(codFeePercent: 2.5m);
        _repository.GetActiveRulesAsync(ServiceType.Standard, default)
            .Returns([rule]);

        var result = await _sut.CalculateAsync(
            new CalculateFeeRequest(ServiceType.Standard, "Hanoi", "Hanoi", 500, 0m));

        result.IsSuccess.Should().BeTrue();
        result.Value.CodFee.Should().Be(0m);
    }
}
