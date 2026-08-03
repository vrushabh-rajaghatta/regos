using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFiledStudySnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FiledStudyIdentifier",
                table: "SubmissionDocuments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FiledStudyTitle",
                table: "SubmissionDocuments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FiledStudyIdentifier",
                table: "SubmissionDocuments");

            migrationBuilder.DropColumn(
                name: "FiledStudyTitle",
                table: "SubmissionDocuments");
        }
    }
}
