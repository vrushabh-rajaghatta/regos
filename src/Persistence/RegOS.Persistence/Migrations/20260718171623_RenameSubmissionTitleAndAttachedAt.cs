using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameSubmissionTitleAndAttachedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Submissions",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "AttachedOnUtc",
                table: "SubmissionDocuments",
                newName: "AttachedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Submissions",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "AttachedAt",
                table: "SubmissionDocuments",
                newName: "AttachedOnUtc");
        }
    }
}
