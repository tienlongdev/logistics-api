using Logistics.Api.BuildingBlocks.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Api.Shipments.Infrastructure.Persistence.Configurations;

internal sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("inbox_messages", "messaging");
        builder.HasKey(x => new { x.Id, x.ConsumerName });

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.ConsumerName).HasColumnName("consumer_name").HasMaxLength(255).IsRequired();
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(512).IsRequired();
        builder.Property(x => x.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.ReceivedOn).HasColumnName("received_on").IsRequired();
        builder.Property(x => x.ProcessedOn).HasColumnName("processed_on");
        builder.Property(x => x.Error).HasColumnName("error");

        builder.HasIndex(x => x.ProcessedOn).HasDatabaseName("ix_inbox_messages_processed_on");
    }
}