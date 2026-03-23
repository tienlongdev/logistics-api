namespace Logistics.Api.Merchants.Infrastructure.Persistence.Entities;

/// <summary>
/// EF Core persistence entity for merchants.merchant_users table.
/// </summary>
internal sealed class MerchantUserEntity
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public Guid UserId { get; set; }
    public string RoleInMerchant { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }

    public MerchantEntity Merchant { get; set; } = default!;
}
