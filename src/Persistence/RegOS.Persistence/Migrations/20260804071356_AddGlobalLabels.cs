using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGlobalLabels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GlobalLabels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    GlobalProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LabelTypeSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LabelTypeCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LabelTypeDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalLabels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlobalLabels_Products_GlobalProductId",
                        column: x => x.GlobalProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GlobalLabelVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ContentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangeSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    PublishedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GlobalLabelId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalLabelVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlobalLabelVersions_GlobalLabels_GlobalLabelId",
                        column: x => x.GlobalLabelId,
                        principalTable: "GlobalLabels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GlobalLabels_GlobalProductId",
                table: "GlobalLabels",
                column: "GlobalProductId");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalLabels_TenantId",
                table: "GlobalLabels",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalLabelVersions_GlobalLabelId_Status",
                table: "GlobalLabelVersions",
                columns: new[] { "GlobalLabelId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_GlobalLabelVersions_GlobalLabelId_VersionNumber",
                table: "GlobalLabelVersions",
                columns: new[] { "GlobalLabelId", "VersionNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GlobalLabelVersions");

            migrationBuilder.DropTable(
                name: "GlobalLabels");
        }
    }
}
