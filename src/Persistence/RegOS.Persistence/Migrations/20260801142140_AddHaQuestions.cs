using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHaQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HaQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Text = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    TargetResponseOn = table.Column<DateOnly>(type: "date", nullable: true),
                    ResponseText = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    CurrentStatus = table.Column<int>(type: "integer", nullable: false),
                    HaCorrespondenceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HaQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HaQuestions_HaCorrespondence_HaCorrespondenceId",
                        column: x => x.HaCorrespondenceId,
                        principalTable: "HaCorrespondence",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HaQuestionStatusEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OccurredOn = table.Column<DateOnly>(type: "date", nullable: false),
                    RecordedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HaQuestionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HaQuestionStatusEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HaQuestionStatusEntries_HaQuestions_HaQuestionId",
                        column: x => x.HaQuestionId,
                        principalTable: "HaQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HaQuestions_HaCorrespondenceId",
                table: "HaQuestions",
                column: "HaCorrespondenceId");

            migrationBuilder.CreateIndex(
                name: "IX_HaQuestionStatusEntries_HaQuestionId",
                table: "HaQuestionStatusEntries",
                column: "HaQuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HaQuestionStatusEntries");

            migrationBuilder.DropTable(
                name: "HaQuestions");
        }
    }
}
