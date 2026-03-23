using Logistics.Api.Notifications.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Api.Notifications.Infrastructure.Persistence.Configurations;

internal sealed class WebhookDeliveryConfiguration : IEntityTypeConfiguration<WebhookDeliveryEntity>
{
    public void Configure(EntityTypeBuilder<WebhookDeliveryEntity> builder)
    {
        builder.ToTable("webhook_deliveries", "notifications");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.SubscriptionId).HasColumnName("subscription_id").IsRequired();
        builder.Property(x => x.MerchantId).HasColumnName("merchant_id").IsRequired();
        builder.Property(x => x.EventType).HasColumnName("event_type").HasMaxLength(100).IsRequired();
        builder.Property(x => x.EventId).HasColumnName("event_id").IsRequired();
        builder.Property(x => x.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(x => x.AttemptCount).HasColumnName("attempt_count").IsRequired();
        builder.Property(x => x.MaxAttempts).HasColumnName("max_attempts").IsRequired();
        builder.Property(x => x.NextRetryAt).HasColumnName("next_retry_at");
        builder.Property(x => x.DeliveredAt).HasColumnName("delivered_at");
        builder.Property(x => x.LastResponseCode).HasColumnName("last_response_code");
        builder.Property(x => x.LastResponseBody).HasColumnName("last_response_body");
        builder.Property(x => x.LastError).HasColumnName("last_error");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(x => new { x.SubscriptionId, x.EventId })
            .IsUnique()
            .HasDatabaseName("ix_webhook_deliveries_subscription_event");
        builder.HasIndex(x => x.MerchantId).HasDatabaseName("ix_webhook_deliveries_merchant_id");
        builder.HasIndex(x => new { x.Status, x.NextRetryAt })
            .HasDatabaseName("ix_webhook_deliveries_status_next_retry_at")
            .HasFilter("status IN ('Pending', 'Failed')");

        builder.HasOne(x => x.Subscription)
            .WithMany()
            .HasForeignKey(x => x.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}