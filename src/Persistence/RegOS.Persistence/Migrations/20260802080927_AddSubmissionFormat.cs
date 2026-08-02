using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionFormat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Backfilled to 1 = SubmissionFormat.Ectd, not to the scaffold's 0.
            // Zero is not a defined value of the enum, so every existing row
            // would have held a format the domain rejects on next write — the
            // same trap S003's PublishedAt migration hit, in the other
            // direction.
            //
            // eCTD is the honest value: every submission in RegOS today belongs
            // to an FDA IND, and FDA has mandated eCTD for those since 2017.
            migrationBuilder.AddColumn<int>(
                name: "Format",
                table: "Submissions",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            // Backfill only. The column keeps no database default, so an insert
            // that omits the format fails loudly instead of silently becoming
            // eCTD — which is the same call Submission.Create makes by taking
            // format as a required parameter (ADR-047).
            migrationBuilder.AlterColumn<int>(
                name: "Format",
                table: "Submissions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Format",
                table: "Submissions");
        }
    }
}
