using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubmissionDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    AttachedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmissionDocuments_DocumentVersions_DocumentVersionId",
                        column: x => x.DocumentVersionId,
                        principalTable: "DocumentVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubmissionDocuments_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionDocuments_DocumentVersionId",
                table: "SubmissionDocuments",
                column: "DocumentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionDocuments_SubmissionId",
                table: "SubmissionDocuments",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionDocuments_SubmissionId_DisplayOrder",
                table: "SubmissionDocuments",
                columns: new[] { "SubmissionId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionDocuments_SubmissionId_ProductDocumentId",
                table: "SubmissionDocuments",
                columns: new[] { "SubmissionId", "ProductDocumentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubmissionDocuments");
        }
    }
}
