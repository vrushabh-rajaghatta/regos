using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalLabels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocalLabels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicinalProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabelTypeSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LabelTypeCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LabelTypeDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalLabels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalLabels_MedicinalProducts_MedicinalProductId",
                        column: x => x.MedicinalProductId,
                        principalTable: "MedicinalProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocalLabelRevisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ContentId = table.Column<Guid>(type: "uuid", nullable: true),
                    DerivedFromGlobalLabelVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    DataCarrierCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ChangeSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ApprovedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    LocalLabelId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalLabelRevisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalLabelRevisions_GlobalLabelVersions_DerivedFromGlobalLa~",
                        column: x => x.DerivedFromGlobalLabelVersionId,
                        principalTable: "GlobalLabelVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LocalLabelRevisions_LocalLabels_LocalLabelId",
                        column: x => x.LocalLabelId,
                        principalTable: "LocalLabels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocalLabelRevisions_DerivedFromGlobalLabelVersionId",
                table: "LocalLabelRevisions",
                column: "DerivedFromGlobalLabelVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalLabelRevisions_LocalLabelId_RevisionNumber",
                table: "LocalLabelRevisions",
                columns: new[] { "LocalLabelId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalLabelRevisions_LocalLabelId_Status",
                table: "LocalLabelRevisions",
                columns: new[] { "LocalLabelId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_LocalLabels_MedicinalProductId",
                table: "LocalLabels",
                column: "MedicinalProductId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalLabels_TenantId",
                table: "LocalLabels",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocalLabelRevisions");

            migrationBuilder.DropTable(
                name: "LocalLabels");
        }
    }
}
