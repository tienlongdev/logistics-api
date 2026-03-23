namespace Logistics.Api.Shipments.Application.Abstractions;

/// <summary>
/// DTO returned by <see cref="IMerchantScopeService"/>.
/// </summary>
public sealed record MerchantInfo(Guid MerchantId, string MerchantCode, string MerchantName);
