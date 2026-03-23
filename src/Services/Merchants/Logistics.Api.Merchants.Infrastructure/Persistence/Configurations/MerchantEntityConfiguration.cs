using Logistics.Api.Merchants.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Api.Merchants.Infrastructure.Persistence.Configurations;

internal sealed class MerchantEntityConfiguration : IEntityTypeConfiguration<MerchantEntity>
{
    public void Configure(EntityTypeBuilder<MerchantEntity> builder)
    {
        builder.ToTable("merchants", "merchants");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(m => m.Code).HasColumnName("code").HasMaxLength(20).IsRequired();
        builder.Property(m => m.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
        builder.Property(m => m.TaxCode).HasColumnName("tax_code").HasMaxLength(50);
        builder.Property(m => m.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        builder.Property(m => m.Phone).HasColumnName("phone").HasMaxLength(20);
        builder.Property(m => m.Address).HasColumnName("address");
        builder.Property(m => m.ApiKey).HasColumnName("api_key").HasMaxLength(512);
        builder.Property(m => m.ApiKeyPrefix).HasColumnName("api_key_prefix").HasMaxLength(10);
        builder.Property(m => m.WebhookSecret).HasColumnName("webhook_secret").HasMaxLength(512);
        builder.Property(m => m.Settings).HasColumnName("settings").HasColumnType("jsonb");
        builder.Property(m => m.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(m => m.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(m => m.Code).IsUnique().HasDatabaseName("ix_merchants_code");
        builder.HasIndex(m => m.Email).IsUnique().HasDatabaseName("ix_merchants_email");
        builder.HasIndex(m => m.ApiKeyPrefix).HasDatabaseName("ix_merchants_api_key_prefix");

        builder.HasMany(m => m.MerchantUsers)
            .WithOne(mu => mu.Merchant)
            .HasForeignKey(mu => mu.MerchantId);
    }
}
