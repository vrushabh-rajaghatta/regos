using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeOrganizationsTenantOwned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Organizations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Backfill, hand-written: every existing organization becomes its
            // same-guid tenant's own registry entry — the mirror convention
            // (ADR-032). Safe universally because AddTenants created an
            // alter-ego tenant for every organization that existed at the
            // split. New organizations are stamped by the create handler.
            migrationBuilder.Sql(
                """
                UPDATE "Organizations" SET "TenantId" = "Id";

                ALTER TABLE "Organizations" ALTER COLUMN "TenantId" DROP DEFAULT;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_TenantId",
                table: "Organizations",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Organizations_TenantId",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Organizations");
        }
    }
}
