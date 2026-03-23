using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Api.Pricing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPricingSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "pricing");

            migrationBuilder.CreateTable(
                name: "pricing_rules",
                schema: "pricing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    service_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    zone_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    from_province = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    to_province = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    min_weight_gram = table.Column<int>(type: "integer", nullable: false),
                    max_weight_gram = table.Column<int>(type: "integer", nullable: true),
                    base_fee = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    per_kg_fee = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    cod_fee_percent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pricing_rules", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_pricing_rules_priority",
                schema: "pricing",
                table: "pricing_rules",
                column: "priority");

            migrationBuilder.CreateIndex(
                name: "ix_pricing_rules_service_type_is_active",
                schema: "pricing",
                table: "pricing_rules",
                columns: new[] { "service_type", "is_active" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pricing_rules",
                schema: "pricing");
        }
    }
}
