using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionTemplateBinding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BoundTemplateVersionId",
                table: "Submissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_BoundTemplateVersionId",
                table: "Submissions",
                column: "BoundTemplateVersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_RegulatoryTemplateVersions_BoundTemplateVersion~",
                table: "Submissions",
                column: "BoundTemplateVersionId",
                principalTable: "RegulatoryTemplateVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_RegulatoryTemplateVersions_BoundTemplateVersion~",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_BoundTemplateVersionId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "BoundTemplateVersionId",
                table: "Submissions");
        }
    }
}
