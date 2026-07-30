using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionDocumentPlacement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TemplateSectionId",
                table: "SubmissionDocuments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionDocuments_TemplateSectionId",
                table: "SubmissionDocuments",
                column: "TemplateSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionDocuments_TemplateSections_TemplateSectionId",
                table: "SubmissionDocuments",
                column: "TemplateSectionId",
                principalTable: "TemplateSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionDocuments_TemplateSections_TemplateSectionId",
                table: "SubmissionDocuments");

            migrationBuilder.DropIndex(
                name: "IX_SubmissionDocuments_TemplateSectionId",
                table: "SubmissionDocuments");

            migrationBuilder.DropColumn(
                name: "TemplateSectionId",
                table: "SubmissionDocuments");
        }
    }
}
