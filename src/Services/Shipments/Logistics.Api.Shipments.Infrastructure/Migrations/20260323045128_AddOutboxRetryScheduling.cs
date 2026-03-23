using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Api.Shipments.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxRetryScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_outbox_messages_status_occurred_on",
                schema: "messaging",
                table: "outbox_messages");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "next_retry_at",
                schema: "messaging",
                table: "outbox_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_status_retry_occurred_on",
                schema: "messaging",
                table: "outbox_messages",
                columns: new[] { "status", "next_retry_at", "occurred_on" },
                filter: "status = 'Pending'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_outbox_messages_status_retry_occurred_on",
                schema: "messaging",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "next_retry_at",
                schema: "messaging",
                table: "outbox_messages");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_status_occurred_on",
                schema: "messaging",
                table: "outbox_messages",
                columns: new[] { "status", "occurred_on" },
                filter: "status = 'Pending'");
        }
    }
}
