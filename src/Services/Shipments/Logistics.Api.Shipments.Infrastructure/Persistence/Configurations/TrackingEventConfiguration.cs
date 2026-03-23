using Logistics.Api.Shipments.Domain.Entities;
using Logistics.Api.Shipments.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Api.Shipments.Infrastructure.Persistence.Configurations;

internal sealed class TrackingEventConfiguration : IEntityTypeConfiguration<TrackingEvent>
{
    public void Configure(EntityTypeBuilder<TrackingEvent> builder)
    {
        builder.ToTable("tracking_events", "shipments");
        builder.HasKey(te => te.Id);

        builder.Property(te => te.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(te => te.ShipmentId).HasColumnName("shipment_id").IsRequired();
        builder.Property(te => te.TrackingCode).HasColumnName("tracking_code").HasMaxLength(20).IsRequired();
        builder.Property(te => te.FromStatus).HasColumnName("from_status").HasMaxLength(50).HasConversion<string?>();
        builder.Property(te => te.ToStatus).HasColumnName("to_status").HasMaxLength(50).HasConversion<string>().IsRequired();
        builder.Property(te => te.HubId).HasColumnName("hub_id");
        builder.Property(te => te.HubCode).HasColumnName("hub_code").HasMaxLength(20);
        builder.Property(te => te.Location).HasColumnName("location").HasMaxLength(255);
        builder.Property(te => te.Note).HasColumnName("note");
        builder.Property(te => te.OperatorId).HasColumnName("operator_id");
        builder.Property(te => te.OperatorName).HasColumnName("operator_name").HasMaxLength(255);
        builder.Property(te => te.Source).HasColumnName("source").HasMaxLength(50).IsRequired();
        builder.Property(te => te.OccurredAt).HasColumnName("occurred_at").IsRequired();
        builder.Property(te => te.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(te => te.ShipmentId).HasDatabaseName("ix_tracking_events_shipment_id");
        builder.HasIndex(te => te.TrackingCode).HasDatabaseName("ix_tracking_events_tracking_code");
        builder.HasIndex(te => te.OccurredAt).HasDatabaseName("ix_tracking_events_occurred_at");
    }
}
