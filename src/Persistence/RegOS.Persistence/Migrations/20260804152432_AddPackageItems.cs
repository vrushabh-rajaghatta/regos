using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPackageItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PackageItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackagedProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentPackageItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    ItemTypeSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ItemTypeCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ItemTypeDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    MaterialSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MaterialCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MaterialDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    UnitOfPresentationSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UnitOfPresentationCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UnitOfPresentationDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageItems_PackageItems_ParentPackageItemId",
                        column: x => x.ParentPackageItemId,
                        principalTable: "PackageItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackageItems_PackagedProducts_PackagedProductId",
                        column: x => x.PackagedProductId,
                        principalTable: "PackagedProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PackageItems_PackagedProductId",
                table: "PackageItems",
                column: "PackagedProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageItems_ParentPackageItemId",
                table: "PackageItems",
                column: "ParentPackageItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageItems_TenantId",
                table: "PackageItems",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackageItems");
        }
    }
}
