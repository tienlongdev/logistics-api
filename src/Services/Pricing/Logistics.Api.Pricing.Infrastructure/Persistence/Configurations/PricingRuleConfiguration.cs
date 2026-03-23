using Logistics.Api.Pricing.Domain.Entities;
using Logistics.Api.Pricing.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Api.Pricing.Infrastructure.Persistence.Configurations;

internal sealed class PricingRuleConfiguration : IEntityTypeConfiguration<PricingRule>
{
    public void Configure(EntityTypeBuilder<PricingRule> builder)
    {
        builder.ToTable("pricing_rules", "pricing");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.ServiceType)
            .HasColumnName("service_type")
            .HasMaxLength(50)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(r => r.ZoneType)
            .HasColumnName("zone_type")
            .HasMaxLength(50)
            .HasConversion<string?>();

        builder.Property(r => r.FromProvince)
            .HasColumnName("from_province")
            .HasMaxLength(100);

        builder.Property(r => r.ToProvince)
            .HasColumnName("to_province")
            .HasMaxLength(100);

        builder.Property(r => r.MinWeightGram)
            .HasColumnName("min_weight_gram")
            .IsRequired();

        builder.Property(r => r.MaxWeightGram)
            .HasColumnName("max_weight_gram");

        builder.Property(r => r.BaseFee)
            .HasColumnName("base_fee")
            .HasColumnType("numeric(15,2)")
            .IsRequired();

        builder.Property(r => r.PerKgFee)
            .HasColumnName("per_kg_fee")
            .HasColumnType("numeric(15,2)")
            .IsRequired();

        builder.Property(r => r.CodFeePercent)
            .HasColumnName("cod_fee_percent")
            .HasColumnType("numeric(5,2)")
            .IsRequired();

        builder.Property(r => r.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(r => r.EffectiveFrom)
            .HasColumnName("effective_from")
            .IsRequired();

        builder.Property(r => r.EffectiveTo)
            .HasColumnName("effective_to");

        builder.Property(r => r.Priority)
            .HasColumnName("priority")
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(r => new { r.ServiceType, r.IsActive })
            .HasDatabaseName("ix_pricing_rules_service_type_is_active");

        builder.HasIndex(r => r.Priority)
            .HasDatabaseName("ix_pricing_rules_priority");
    }
}
