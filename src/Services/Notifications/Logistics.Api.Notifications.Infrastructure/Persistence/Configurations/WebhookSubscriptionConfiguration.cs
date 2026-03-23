using Logistics.Api.Notifications.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Api.Notifications.Infrastructure.Persistence.Configurations;

internal sealed class WebhookSubscriptionConfiguration : IEntityTypeConfiguration<WebhookSubscriptionEntity>
{
    public void Configure(EntityTypeBuilder<WebhookSubscriptionEntity> builder)
    {
        builder.ToTable("webhook_subscriptions", "notifications");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.MerchantId).HasColumnName("merchant_id").IsRequired();
        builder.Property(x => x.CallbackUrl).HasColumnName("callback_url").HasMaxLength(2048).IsRequired();
        builder.Property(x => x.Events).HasColumnName("events").HasColumnType("text[]").IsRequired();
        builder.Property(x => x.Secret).HasColumnName("secret").HasMaxLength(512).IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(x => x.MerchantId).HasDatabaseName("ix_webhook_subscriptions_merchant_id");
    }
}