using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessStepExecution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentStatus",
                table: "ProcessSteps",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ProcessStepStatusHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OccurredOn = table.Column<DateOnly>(type: "date", nullable: false),
                    RecordedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ProcessStepId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessStepStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessStepStatusHistory_ProcessSteps_ProcessStepId",
                        column: x => x.ProcessStepId,
                        principalTable: "ProcessSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessSteps_CurrentStatus_PlannedStartOn",
                table: "ProcessSteps",
                columns: new[] { "CurrentStatus", "PlannedStartOn" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessStepStatusHistory_ProcessStepId",
                table: "ProcessStepStatusHistory",
                column: "ProcessStepId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessStepStatusHistory");

            migrationBuilder.DropIndex(
                name: "IX_ProcessSteps_CurrentStatus_PlannedStartOn",
                table: "ProcessSteps");

            migrationBuilder.DropColumn(
                name: "CurrentStatus",
                table: "ProcessSteps");
        }
    }
}
