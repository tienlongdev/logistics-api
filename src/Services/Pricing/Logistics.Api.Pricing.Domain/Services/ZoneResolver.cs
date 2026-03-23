using Logistics.Api.Pricing.Domain.Enums;

namespace Logistics.Api.Pricing.Domain.Services;

/// <summary>
/// Resolves the zone type for a shipment from its sender/receiver province names.
///
/// MVP rule:
///   - sender province == receiver province → SameProvince
///   - otherwise → National
///
/// NearBy is reserved for a future rule (adjacency matrix) but is not used in MVP.
/// Province comparison is case-insensitive, trimmed.
/// </summary>
public static class ZoneResolver
{
    public static ZoneType Resolve(string? senderProvince, string? receiverProvince)
    {
        if (string.IsNullOrWhiteSpace(senderProvince) || string.IsNullOrWhiteSpace(receiverProvince))
            return ZoneType.National;

        return string.Equals(
                senderProvince.Trim(),
                receiverProvince.Trim(),
                StringComparison.OrdinalIgnoreCase)
            ? ZoneType.SameProvince
            : ZoneType.National;
    }
}
