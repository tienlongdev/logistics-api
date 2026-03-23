using Logistics.Api.BuildingBlocks.Infrastructure.Messaging;
using Logistics.Api.Notifications.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api.Notifications.Infrastructure.Persistence;

public sealed class NotificationsDbContext : DbContext
{
    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options) { }

    public DbSet<WebhookSubscriptionEntity> WebhookSubscriptions => Set<WebhookSubscriptionEntity>();
    public DbSet<WebhookDeliveryEntity> WebhookDeliveries => Set<WebhookDeliveryEntity>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("notifications");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}