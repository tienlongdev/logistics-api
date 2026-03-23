namespace Logistics.Api.Merchants.Infrastructure.Persistence.Entities;

/// <summary>
/// EF Core persistence entity for merchants.merchants table.
/// This is an infrastructure concern — not a domain model.
/// </summary>
internal sealed class MerchantEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? TaxCode { get; set; }
    public string Email { get; set; } = default!;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiKeyPrefix { get; set; }
    public string? WebhookSecret { get; set; }
    public string? Settings { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<MerchantUserEntity> MerchantUsers { get; set; } = [];
}
