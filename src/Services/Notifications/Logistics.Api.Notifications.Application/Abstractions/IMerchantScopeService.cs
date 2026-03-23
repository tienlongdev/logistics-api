namespace Logistics.Api.Notifications.Application.Abstractions;

public interface IMerchantScopeService
{
    Task<MerchantInfo?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}