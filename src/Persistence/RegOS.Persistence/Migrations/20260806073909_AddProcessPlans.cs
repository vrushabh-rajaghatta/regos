using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessObjectiveId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessDefinitionVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    AnchorDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CurrentStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessPlans_ProcessDefinitionVersions_ProcessDefinitionVer~",
                        column: x => x.ProcessDefinitionVersionId,
                        principalTable: "ProcessDefinitionVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessPlans_ProcessObjectives_ProcessObjectiveId",
                        column: x => x.ProcessObjectiveId,
                        principalTable: "ProcessObjectives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessPlanStatusHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OccurredOn = table.Column<DateOnly>(type: "date", nullable: false),
                    RecordedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ProcessPlanId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessPlanStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessPlanStatusHistory_ProcessPlans_ProcessPlanId",
                        column: x => x.ProcessPlanId,
                        principalTable: "ProcessPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StepDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ParentStepId = table.Column<Guid>(type: "uuid", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    PlannedStartOn = table.Column<DateOnly>(type: "date", nullable: false),
                    PlannedEndOn = table.Column<DateOnly>(type: "date", nullable: false),
                    ProcessPlanId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessSteps_ProcessPlans_ProcessPlanId",
                        column: x => x.ProcessPlanId,
                        principalTable: "ProcessPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessStepDependencies",
                columns: table => new
                {
                    ProcessStepId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PredecessorStepId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessStepDependencies", x => new { x.ProcessStepId, x.Id });
                    table.ForeignKey(
                        name: "FK_ProcessStepDependencies_ProcessSteps_ProcessStepId",
                        column: x => x.ProcessStepId,
                        principalTable: "ProcessSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessPlans_ProcessDefinitionVersionId",
                table: "ProcessPlans",
                column: "ProcessDefinitionVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessPlans_ProcessObjectiveId",
                table: "ProcessPlans",
                column: "ProcessObjectiveId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessPlans_TenantId_CurrentStatus",
                table: "ProcessPlans",
                columns: new[] { "TenantId", "CurrentStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessPlanStatusHistory_ProcessPlanId",
                table: "ProcessPlanStatusHistory",
                column: "ProcessPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessStepDependencies_ProcessStepId_PredecessorStepId",
                table: "ProcessStepDependencies",
                columns: new[] { "ProcessStepId", "PredecessorStepId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessSteps_ProcessPlanId_Code",
                table: "ProcessSteps",
                columns: new[] { "ProcessPlanId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessSteps_ProcessPlanId_PlannedStartOn",
                table: "ProcessSteps",
                columns: new[] { "ProcessPlanId", "PlannedStartOn" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessPlanStatusHistory");

            migrationBuilder.DropTable(
                name: "ProcessStepDependencies");

            migrationBuilder.DropTable(
                name: "ProcessSteps");

            migrationBuilder.DropTable(
                name: "ProcessPlans");
        }
    }
}
