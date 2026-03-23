namespace Logistics.Api.Shipments.Domain.Enums;

/// <summary>
/// Mirrors <c>Logistics.Api.Pricing.Domain.Enums.ServiceType</c> intentionally.
/// The Shipments domain owns this enum; Pricing domain owns its own copy.
/// Numeric values are identical to allow safe casting between the two.
/// </summary>
public enum ServiceType
{
    Standard = 1,
    Express = 2,
}
