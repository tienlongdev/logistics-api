using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Api.Shipments.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialShipmentsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "shipments");

            migrationBuilder.CreateTable(
                name: "shipments",
                schema: "shipments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tracking_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    shipment_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sender_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    sender_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sender_address = table.Column<string>(type: "text", nullable: false),
                    sender_province = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sender_district = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sender_ward = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    receiver_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    receiver_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    receiver_address = table.Column<string>(type: "text", nullable: false),
                    receiver_province = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    receiver_district = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    receiver_ward = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    weight_gram = table.Column<int>(type: "integer", nullable: false),
                    length_cm = table.Column<int>(type: "integer", nullable: true),
                    width_cm = table.Column<int>(type: "integer", nullable: true),
                    height_cm = table.Column<int>(type: "integer", nullable: true),
                    package_description = table.Column<string>(type: "text", nullable: true),
                    declared_value = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    cod_amount = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    shipping_fee = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    insurance_fee = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    total_fee = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    service_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    delivery_note = table.Column<string>(type: "text", nullable: true),
                    current_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    current_hub_id = table.Column<Guid>(type: "uuid", nullable: true),
                    current_hub_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    expected_delivery = table.Column<DateOnly>(type: "date", nullable: true),
                    actual_delivered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancelled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancelled_reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tracking_events",
                schema: "shipments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tracking_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    from_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    to_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    hub_id = table.Column<Guid>(type: "uuid", nullable: true),
                    hub_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    operator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    operator_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tracking_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_tracking_events_shipments_shipment_id",
                        column: x => x.shipment_id,
                        principalSchema: "shipments",
                        principalTable: "shipments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_shipments_created_at",
                schema: "shipments",
                table: "shipments",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_shipments_current_status",
                schema: "shipments",
                table: "shipments",
                column: "current_status");

            migrationBuilder.CreateIndex(
                name: "ix_shipments_idempotency_key",
                schema: "shipments",
                table: "shipments",
                column: "idempotency_key",
                unique: true,
                filter: "idempotency_key IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_shipments_merchant_id",
                schema: "shipments",
                table: "shipments",
                column: "merchant_id");

            migrationBuilder.CreateIndex(
                name: "ix_shipments_merchant_status",
                schema: "shipments",
                table: "shipments",
                columns: new[] { "merchant_id", "current_status" });

            migrationBuilder.CreateIndex(
                name: "ix_shipments_receiver_phone",
                schema: "shipments",
                table: "shipments",
                column: "receiver_phone");

            migrationBuilder.CreateIndex(
                name: "ix_shipments_shipment_code",
                schema: "shipments",
                table: "shipments",
                column: "shipment_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shipments_tracking_code",
                schema: "shipments",
                table: "shipments",
                column: "tracking_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tracking_events_occurred_at",
                schema: "shipments",
                table: "tracking_events",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "ix_tracking_events_shipment_id",
                schema: "shipments",
                table: "tracking_events",
                column: "shipment_id");

            migrationBuilder.CreateIndex(
                name: "ix_tracking_events_tracking_code",
                schema: "shipments",
                table: "tracking_events",
                column: "tracking_code");

            // Sequence used to generate sequential tracking/shipment codes
            migrationBuilder.Sql(
                "CREATE SEQUENCE shipments.shipment_number_seq START 1 INCREMENT 1 NO CYCLE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS shipments.shipment_number_seq;");

            migrationBuilder.DropTable(
                name: "tracking_events",
                schema: "shipments");

            migrationBuilder.DropTable(
                name: "shipments",
                schema: "shipments");
        }
    }
}
