using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyRegistry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClinicalStudies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SponsorStudyIdentifier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalStudies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NonClinicalStudies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SponsorStudyIdentifier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NonClinicalStudies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalStudies_TenantId_SponsorStudyIdentifier",
                table: "ClinicalStudies",
                columns: new[] { "TenantId", "SponsorStudyIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NonClinicalStudies_TenantId_SponsorStudyIdentifier",
                table: "NonClinicalStudies",
                columns: new[] { "TenantId", "SponsorStudyIdentifier" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClinicalStudies");

            migrationBuilder.DropTable(
                name: "NonClinicalStudies");
        }
    }
}
