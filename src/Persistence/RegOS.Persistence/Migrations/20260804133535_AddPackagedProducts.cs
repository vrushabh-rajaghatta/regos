using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPackagedProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PackagedProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicinalProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    PackSizeQuantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    PackSizeUnitSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PackSizeUnitCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PackSizeUnitDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    PackCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CurrentMarketingStatus = table.Column<int>(type: "integer", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackagedProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackagedProducts_MedicinalProducts_MedicinalProductId",
                        column: x => x.MedicinalProductId,
                        principalTable: "MedicinalProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageMarketingStatusHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OccurredOn = table.Column<DateOnly>(type: "date", nullable: false),
                    RecordedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PackagedProductId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageMarketingStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageMarketingStatusHistory_PackagedProducts_PackagedProd~",
                        column: x => x.PackagedProductId,
                        principalTable: "PackagedProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PackagedProducts_MedicinalProductId",
                table: "PackagedProducts",
                column: "MedicinalProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagedProducts_MedicinalProductId_CurrentMarketingStatus",
                table: "PackagedProducts",
                columns: new[] { "MedicinalProductId", "CurrentMarketingStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_PackagedProducts_PackCode",
                table: "PackagedProducts",
                column: "PackCode");

            migrationBuilder.CreateIndex(
                name: "IX_PackagedProducts_TenantId",
                table: "PackagedProducts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageMarketingStatusHistory_PackagedProductId",
                table: "PackageMarketingStatusHistory",
                column: "PackagedProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageMarketingStatusHistory_PackagedProductId_OccurredOn",
                table: "PackageMarketingStatusHistory",
                columns: new[] { "PackagedProductId", "OccurredOn" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackageMarketingStatusHistory");

            migrationBuilder.DropTable(
                name: "PackagedProducts");
        }
    }
}
