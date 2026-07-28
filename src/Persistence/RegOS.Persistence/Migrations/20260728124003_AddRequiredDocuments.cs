using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRequiredDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequiredDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    RegulatoryTemplateVersionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequiredDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequiredDocuments_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequiredDocuments_RegulatoryTemplateVersions_RegulatoryTemp~",
                        column: x => x.RegulatoryTemplateVersionId,
                        principalTable: "RegulatoryTemplateVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequiredDocuments_DocumentTypeId",
                table: "RequiredDocuments",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RequiredDocuments_RegulatoryTemplateVersionId",
                table: "RequiredDocuments",
                column: "RegulatoryTemplateVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_RequiredDocuments_SectionId_DocumentTypeId",
                table: "RequiredDocuments",
                columns: new[] { "SectionId", "DocumentTypeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequiredDocuments");
        }
    }
}
