namespace Logistics.Api.Pricing.Domain.Enums;

/// <summary>
/// Zone type used to match a pricing rule against a shipment's origin/destination.
/// MVP scope: SameProvince vs National.
/// NearBy is defined for future use (adjacent provinces).
/// </summary>
public enum ZoneType
{
    SameProvince = 1,
    National = 2,
    NearBy = 3
}
