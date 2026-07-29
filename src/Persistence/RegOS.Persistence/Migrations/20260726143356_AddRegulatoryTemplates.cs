using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRegulatoryTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegulatoryTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AuthorityId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Source = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegulatoryTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegulatoryTemplates_Authorities_AuthorityId",
                        column: x => x.AuthorityId,
                        principalTable: "Authorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegulatoryTemplates_SubmissionTypes_SubmissionTypeId",
                        column: x => x.SubmissionTypeId,
                        principalTable: "SubmissionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegulatoryTemplates_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegulatoryTemplateVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    PublishedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RegulatoryTemplateId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegulatoryTemplateVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegulatoryTemplateVersions_RegulatoryTemplates_RegulatoryTe~",
                        column: x => x.RegulatoryTemplateId,
                        principalTable: "RegulatoryTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryTemplates_AuthorityId",
                table: "RegulatoryTemplates",
                column: "AuthorityId");

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryTemplates_Code",
                table: "RegulatoryTemplates",
                column: "Code",
                unique: true,
                filter: "\"TenantId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryTemplates_SubmissionTypeId",
                table: "RegulatoryTemplates",
                column: "SubmissionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryTemplates_TenantId",
                table: "RegulatoryTemplates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryTemplateVersions_RegulatoryTemplateId",
                table: "RegulatoryTemplateVersions",
                column: "RegulatoryTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryTemplateVersions_RegulatoryTemplateId_VersionNumb~",
                table: "RegulatoryTemplateVersions",
                columns: new[] { "RegulatoryTemplateId", "VersionNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegulatoryTemplateVersions");

            migrationBuilder.DropTable(
                name: "RegulatoryTemplates");
        }
    }
}
