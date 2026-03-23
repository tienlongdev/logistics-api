using FluentAssertions;
using Logistics.Api.Pricing.Domain.Enums;
using Logistics.Api.Pricing.Domain.Services;

namespace Logistics.Api.UnitTests.Pricing;

public sealed class ZoneResolverTests
{
    [Fact]
    public void Resolve_ReturnsSameProvince_WhenBothProvincesAreEqual()
    {
        ZoneResolver.Resolve("Hanoi", "Hanoi").Should().Be(ZoneType.SameProvince);
    }

    [Fact]
    public void Resolve_ReturnsSameProvince_CaseInsensitive()
    {
        ZoneResolver.Resolve("hanoi", "HANOI").Should().Be(ZoneType.SameProvince);
    }

    [Fact]
    public void Resolve_ReturnsSameProvince_IgnoresLeadingTrailingWhitespace()
    {
        ZoneResolver.Resolve("  Hanoi  ", "Hanoi").Should().Be(ZoneType.SameProvince);
    }

    [Fact]
    public void Resolve_ReturnsNational_WhenProvincesAreDifferent()
    {
        ZoneResolver.Resolve("Hanoi", "Ho Chi Minh").Should().Be(ZoneType.National);
    }

    [Fact]
    public void Resolve_ReturnsNational_WhenSenderProvinceIsNull()
    {
        ZoneResolver.Resolve(null, "Hanoi").Should().Be(ZoneType.National);
    }

    [Fact]
    public void Resolve_ReturnsNational_WhenReceiverProvinceIsNull()
    {
        ZoneResolver.Resolve("Hanoi", null).Should().Be(ZoneType.National);
    }

    [Fact]
    public void Resolve_ReturnsNational_WhenBothProvincesAreNull()
    {
        ZoneResolver.Resolve(null, null).Should().Be(ZoneType.National);
    }

    [Fact]
    public void Resolve_ReturnsNational_WhenSenderProvinceIsEmpty()
    {
        ZoneResolver.Resolve("", "Hanoi").Should().Be(ZoneType.National);
    }

    [Fact]
    public void Resolve_ReturnsNational_WhenSenderProvinceIsWhitespace()
    {
        ZoneResolver.Resolve("   ", "Hanoi").Should().Be(ZoneType.National);
    }
}
