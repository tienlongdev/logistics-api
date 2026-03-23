using Logistics.Api.Pricing.Domain.Enums;

namespace Logistics.Api.Pricing.Application.Services;

/// <summary>
/// Input data required to calculate shipping fees for a shipment.
/// This record is the contract between the Pricing module and callers (e.g. Shipments module).
/// </summary>
public sealed record CalculateFeeRequest(
    ServiceType ServiceType,
    string? SenderProvince,
    string? ReceiverProvince,
    int WeightGram,
    decimal CodAmount);
