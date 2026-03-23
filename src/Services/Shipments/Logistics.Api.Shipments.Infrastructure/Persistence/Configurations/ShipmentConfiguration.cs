using Logistics.Api.Shipments.Domain.Entities;
using Logistics.Api.Shipments.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Api.Shipments.Infrastructure.Persistence.Configurations;

internal sealed class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("shipments", "shipments");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).HasColumnName("id").ValueGeneratedNever();

        // Identity
        builder.Property(s => s.TrackingCode).HasColumnName("tracking_code").HasMaxLength(20).IsRequired();
        builder.Property(s => s.ShipmentCode).HasColumnName("shipment_code").HasMaxLength(30).IsRequired();
        builder.Property(s => s.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(255);

        // Merchant
        builder.Property(s => s.MerchantId).HasColumnName("merchant_id").IsRequired();
        builder.Property(s => s.MerchantCode).HasColumnName("merchant_code").HasMaxLength(20).IsRequired();

        // Sender
        builder.Property(s => s.SenderName).HasColumnName("sender_name").HasMaxLength(255).IsRequired();
        builder.Property(s => s.SenderPhone).HasColumnName("sender_phone").HasMaxLength(20).IsRequired();
        builder.Property(s => s.SenderAddress).HasColumnName("sender_address").IsRequired();
        builder.Property(s => s.SenderProvince).HasColumnName("sender_province").HasMaxLength(100);
        builder.Property(s => s.SenderDistrict).HasColumnName("sender_district").HasMaxLength(100);
        builder.Property(s => s.SenderWard).HasColumnName("sender_ward").HasMaxLength(100);

        // Receiver
        builder.Property(s => s.ReceiverName).HasColumnName("receiver_name").HasMaxLength(255).IsRequired();
        builder.Property(s => s.ReceiverPhone).HasColumnName("receiver_phone").HasMaxLength(20).IsRequired();
        builder.Property(s => s.ReceiverAddress).HasColumnName("receiver_address").IsRequired();
        builder.Property(s => s.ReceiverProvince).HasColumnName("receiver_province").HasMaxLength(100);
        builder.Property(s => s.ReceiverDistrict).HasColumnName("receiver_district").HasMaxLength(100);
        builder.Property(s => s.ReceiverWard).HasColumnName("receiver_ward").HasMaxLength(100);

        // Package
        builder.Property(s => s.WeightGram).HasColumnName("weight_gram").IsRequired();
        builder.Property(s => s.LengthCm).HasColumnName("length_cm");
        builder.Property(s => s.WidthCm).HasColumnName("width_cm");
        builder.Property(s => s.HeightCm).HasColumnName("height_cm");
        builder.Property(s => s.PackageDescription).HasColumnName("package_description");
        builder.Property(s => s.DeclaredValue).HasColumnName("declared_value").HasColumnType("numeric(15,2)").IsRequired();

        // Financial
        builder.Property(s => s.CodAmount).HasColumnName("cod_amount").HasColumnType("numeric(15,2)").IsRequired();
        builder.Property(s => s.ShippingFee).HasColumnName("shipping_fee").HasColumnType("numeric(15,2)").IsRequired();
        builder.Property(s => s.InsuranceFee).HasColumnName("insurance_fee").HasColumnType("numeric(15,2)").IsRequired();
        builder.Property(s => s.TotalFee).HasColumnName("total_fee").HasColumnType("numeric(15,2)").IsRequired();

        // Service
        builder.Property(s => s.ServiceType).HasColumnName("service_type").HasMaxLength(50).HasConversion<string>().IsRequired();
        builder.Property(s => s.DeliveryNote).HasColumnName("delivery_note");

        // Status & hub
        builder.Property(s => s.CurrentStatus).HasColumnName("current_status").HasMaxLength(50).HasConversion<string>().IsRequired();
        builder.Property(s => s.CurrentHubId).HasColumnName("current_hub_id");
        builder.Property(s => s.CurrentHubCode).HasColumnName("current_hub_code").HasMaxLength(20);

        // Lifecycle
        builder.Property(s => s.ExpectedDelivery).HasColumnName("expected_delivery");
        builder.Property(s => s.ActualDeliveredAt).HasColumnName("actual_delivered_at");
        builder.Property(s => s.CancelledAt).HasColumnName("cancelled_at");
        builder.Property(s => s.CancelledReason).HasColumnName("cancelled_reason");

        // Metadata
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at").IsRequired();

        // Unique indexes
        builder.HasIndex(s => s.TrackingCode).IsUnique().HasDatabaseName("ix_shipments_tracking_code");
        builder.HasIndex(s => s.ShipmentCode).IsUnique().HasDatabaseName("ix_shipments_shipment_code");
        builder.HasIndex(s => s.IdempotencyKey).IsUnique().HasDatabaseName("ix_shipments_idempotency_key")
            .HasFilter("idempotency_key IS NOT NULL");

        // Query indexes
        builder.HasIndex(s => s.MerchantId).HasDatabaseName("ix_shipments_merchant_id");
        builder.HasIndex(s => s.CurrentStatus).HasDatabaseName("ix_shipments_current_status");
        builder.HasIndex(s => s.ReceiverPhone).HasDatabaseName("ix_shipments_receiver_phone");
        builder.HasIndex(s => s.CreatedAt).HasDatabaseName("ix_shipments_created_at");
        builder.HasIndex(s => new { s.MerchantId, s.CurrentStatus })
            .HasDatabaseName("ix_shipments_merchant_status");

        // Navigation
        builder.HasMany(s => s.TrackingEvents)
            .WithOne()
            .HasForeignKey(te => te.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.TrackingEvents)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
