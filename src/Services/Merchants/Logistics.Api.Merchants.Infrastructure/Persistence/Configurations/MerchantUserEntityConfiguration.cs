using Logistics.Api.Merchants.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Api.Merchants.Infrastructure.Persistence.Configurations;

internal sealed class MerchantUserEntityConfiguration : IEntityTypeConfiguration<MerchantUserEntity>
{
    public void Configure(EntityTypeBuilder<MerchantUserEntity> builder)
    {
        builder.ToTable("merchant_users", "merchants");
        builder.HasKey(mu => mu.Id);

        builder.Property(mu => mu.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(mu => mu.MerchantId).HasColumnName("merchant_id").IsRequired();
        builder.Property(mu => mu.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(mu => mu.RoleInMerchant).HasColumnName("role_in_merchant").HasMaxLength(50).IsRequired();
        builder.Property(mu => mu.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(mu => new { mu.MerchantId, mu.UserId }).IsUnique()
            .HasDatabaseName("ix_merchant_users_merchant_user");
        builder.HasIndex(mu => mu.UserId)
            .HasDatabaseName("ix_merchant_users_user_id");
    }
}
