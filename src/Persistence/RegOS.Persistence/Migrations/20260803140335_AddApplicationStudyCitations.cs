using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationStudyCitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationStudyCitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicalStudyId = table.Column<Guid>(type: "uuid", nullable: true),
                    NonClinicalStudyId = table.Column<Guid>(type: "uuid", nullable: true),
                    CitedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationStudyCitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationStudyCitations_ClinicalStudies_ClinicalStudyId",
                        column: x => x.ClinicalStudyId,
                        principalTable: "ClinicalStudies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApplicationStudyCitations_NonClinicalStudies_NonClinicalStu~",
                        column: x => x.NonClinicalStudyId,
                        principalTable: "NonClinicalStudies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApplicationStudyCitations_RegulatoryApplications_Applicatio~",
                        column: x => x.ApplicationId,
                        principalTable: "RegulatoryApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationStudyCitations_ApplicationId_ClinicalStudyId",
                table: "ApplicationStudyCitations",
                columns: new[] { "ApplicationId", "ClinicalStudyId" },
                unique: true,
                filter: "\"ClinicalStudyId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationStudyCitations_ApplicationId_NonClinicalStudyId",
                table: "ApplicationStudyCitations",
                columns: new[] { "ApplicationId", "NonClinicalStudyId" },
                unique: true,
                filter: "\"NonClinicalStudyId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationStudyCitations_ClinicalStudyId",
                table: "ApplicationStudyCitations",
                column: "ClinicalStudyId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationStudyCitations_NonClinicalStudyId",
                table: "ApplicationStudyCitations",
                column: "NonClinicalStudyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationStudyCitations");
        }
    }
}
