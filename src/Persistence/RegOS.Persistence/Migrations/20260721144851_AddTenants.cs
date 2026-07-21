using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentTypes_Organizations_OrganizationId",
                table: "DocumentTypes");

            migrationBuilder.DropIndex(
                name: "IX_DocumentTypes_Code",
                table: "DocumentTypes");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "Users",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_OrganizationId",
                table: "Users",
                newName: "IX_Users_TenantId");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "Products",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_OrganizationId_Code",
                table: "Products",
                newName: "IX_Products_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_Products_OrganizationId",
                table: "Products",
                newName: "IX_Products_TenantId");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "DocumentTypes",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentTypes_OrganizationId",
                table: "DocumentTypes",
                newName: "IX_DocumentTypes_TenantId");

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            // Backfill, hand-written (EF cannot scaffold data movement): every
            // organization was a tenant under the fused model (ADR-015), so
            // each becomes a Tenant row with the SAME id. Preserving ids is
            // what keeps every renamed TenantId column on Users, Products and
            // DocumentTypes pointing at a row that exists — and is why this
            // INSERT must run before the DocumentTypes FK below is created.
            // Status values map 1:1 (Active=1, Inactive=2 in both enums).
            migrationBuilder.Sql(
                """
                INSERT INTO "Tenants" ("Id", "Name", "Status")
                SELECT "Id", "LegalName", "Status"
                FROM "Organizations";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_Code",
                table: "DocumentTypes",
                column: "Code",
                unique: true,
                filter: "\"TenantId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Name",
                table: "Tenants",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentTypes_Tenants_TenantId",
                table: "DocumentTypes",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentTypes_Tenants_TenantId",
                table: "DocumentTypes");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_DocumentTypes_Code",
                table: "DocumentTypes");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "Users",
                newName: "OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                newName: "IX_Users_OrganizationId");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "Products",
                newName: "OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_TenantId_Code",
                table: "Products",
                newName: "IX_Products_OrganizationId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_Products_TenantId",
                table: "Products",
                newName: "IX_Products_OrganizationId");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "DocumentTypes",
                newName: "OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentTypes_TenantId",
                table: "DocumentTypes",
                newName: "IX_DocumentTypes_OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_Code",
                table: "DocumentTypes",
                column: "Code",
                unique: true,
                filter: "\"OrganizationId\" IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentTypes_Organizations_OrganizationId",
                table: "DocumentTypes",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
