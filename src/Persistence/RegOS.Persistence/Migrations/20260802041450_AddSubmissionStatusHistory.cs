using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubmissionStatusEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OccurredOn = table.Column<DateOnly>(type: "date", nullable: false),
                    RecordedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionStatusEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmissionStatusEntries_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionStatusEntries_SubmissionId",
                table: "SubmissionStatusEntries",
                column: "SubmissionId");

            // Backfill before dropping the column, not after. Every submission
            // that already exists became a draft when it was created, and the
            // published ones were published at PublishedAt — those are real
            // events, and a history that began the day this migration ran would
            // be a worse record than the one it replaced.
            //
            // OccurredOn is the date part: PublishedAt is when RegOS was told,
            // and for a submission published through RegOS those are the same
            // day. Nothing better is recoverable, and inventing precision the
            // old column never carried would be the wrong kind of accurate.
            migrationBuilder.Sql("""
                INSERT INTO "SubmissionStatusEntries"
                    ("Id", "SubmissionId", "Status", "OccurredOn", "RecordedOnUtc", "Note")
                SELECT gen_random_uuid(), s."Id", 1,
                       (s."CreatedOn" AT TIME ZONE 'UTC')::date, s."CreatedOn", NULL
                FROM "Submissions" s;
                """);

            migrationBuilder.Sql("""
                INSERT INTO "SubmissionStatusEntries"
                    ("Id", "SubmissionId", "Status", "OccurredOn", "RecordedOnUtc", "Note")
                SELECT gen_random_uuid(), s."Id", 2,
                       (s."PublishedAt" AT TIME ZONE 'UTC')::date, s."PublishedAt", NULL
                FROM "Submissions" s
                WHERE s."PublishedAt" IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "Submissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubmissionStatusEntries");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PublishedAt",
                table: "Submissions",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
