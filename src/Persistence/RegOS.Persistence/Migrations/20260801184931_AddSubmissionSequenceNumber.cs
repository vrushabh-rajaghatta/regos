using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionSequenceNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SequenceNumber",
                table: "Submissions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ApplicationId_SequenceNumber",
                table: "Submissions",
                columns: new[] { "ApplicationId", "SequenceNumber" },
                unique: true,
                filter: "\"SequenceNumber\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Submissions_ApplicationId_SequenceNumber",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "SequenceNumber",
                table: "Submissions");
        }
    }
}
