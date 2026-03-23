using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Api.Merchants.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMerchantsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "merchants");

            migrationBuilder.CreateTable(
                name: "merchants",
                schema: "merchants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    tax_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    api_key = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    api_key_prefix = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    webhook_secret = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    settings = table.Column<string>(type: "jsonb", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merchants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "merchant_users",
                schema: "merchants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_in_merchant = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merchant_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_merchant_users_merchants_merchant_id",
                        column: x => x.merchant_id,
                        principalSchema: "merchants",
                        principalTable: "merchants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_merchant_users_merchant_user",
                schema: "merchants",
                table: "merchant_users",
                columns: new[] { "merchant_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_merchant_users_user_id",
                schema: "merchants",
                table: "merchant_users",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_merchants_api_key_prefix",
                schema: "merchants",
                table: "merchants",
                column: "api_key_prefix");

            migrationBuilder.CreateIndex(
                name: "ix_merchants_code",
                schema: "merchants",
                table: "merchants",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_merchants_email",
                schema: "merchants",
                table: "merchants",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "merchant_users",
                schema: "merchants");

            migrationBuilder.DropTable(
                name: "merchants",
                schema: "merchants");
        }
    }
}
