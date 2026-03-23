using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Api.Shipments.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMessagingInboxAndOutboxRetries : Migration
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

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "messaging",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    consumer_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    type = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    received_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    processed_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_messages", x => new { x.id, x.consumer_name });
                });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_status_retry_occurred_on",
                schema: "messaging",
                table: "outbox_messages",
                columns: new[] { "status", "next_retry_at", "occurred_on" },
                filter: "status = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "ix_inbox_messages_processed_on",
                schema: "messaging",
                table: "inbox_messages",
                column: "processed_on");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "messaging");

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
