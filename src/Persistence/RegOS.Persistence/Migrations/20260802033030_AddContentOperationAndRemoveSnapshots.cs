using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContentOperationAndRemoveSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubmissionSnapshotDocuments");

            migrationBuilder.DropTable(
                name: "SubmissionSnapshots");

            migrationBuilder.AddColumn<int>(
                name: "Operation",
                table: "SubmissionDocuments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReplacesSubmissionDocumentId",
                table: "SubmissionDocuments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SubmissionDeletions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateSectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletesSubmissionDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionDeletions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmissionDeletions_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionDeletions_SubmissionId",
                table: "SubmissionDeletions",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionDeletions_SubmissionId_ProductDocumentId_Template~",
                table: "SubmissionDeletions",
                columns: new[] { "SubmissionId", "ProductDocumentId", "TemplateSectionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubmissionDeletions");

            migrationBuilder.DropColumn(
                name: "Operation",
                table: "SubmissionDocuments");

            migrationBuilder.DropColumn(
                name: "ReplacesSubmissionDocumentId",
                table: "SubmissionDocuments");

            migrationBuilder.CreateTable(
                name: "SubmissionSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmissionSnapshots_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionSnapshotDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    DocumentVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionSnapshotId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionSnapshotDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmissionSnapshotDocuments_DocumentVersions_DocumentVersio~",
                        column: x => x.DocumentVersionId,
                        principalTable: "DocumentVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubmissionSnapshotDocuments_SubmissionSnapshots_SubmissionS~",
                        column: x => x.SubmissionSnapshotId,
                        principalTable: "SubmissionSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionSnapshotDocuments_DocumentVersionId",
                table: "SubmissionSnapshotDocuments",
                column: "DocumentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionSnapshotDocuments_SubmissionSnapshotId",
                table: "SubmissionSnapshotDocuments",
                column: "SubmissionSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionSnapshotDocuments_SubmissionSnapshotId_DisplayOrd~",
                table: "SubmissionSnapshotDocuments",
                columns: new[] { "SubmissionSnapshotId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionSnapshots_SubmissionId",
                table: "SubmissionSnapshots",
                column: "SubmissionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionSnapshots_TenantId",
                table: "SubmissionSnapshots",
                column: "TenantId");
        }
    }
}
