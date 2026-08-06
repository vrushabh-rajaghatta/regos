using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessDefinitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessDefinitionVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    PublishedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessDefinitionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessDefinitionVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessDefinitionVersions_ProcessDefinitions_ProcessDefinit~",
                        column: x => x.ProcessDefinitionId,
                        principalTable: "ProcessDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessStepDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ParentStepId = table.Column<Guid>(type: "uuid", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    OffsetDays = table.Column<int>(type: "integer", nullable: false),
                    DurationDays = table.Column<int>(type: "integer", nullable: false),
                    ProcessDefinitionVersionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessStepDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessStepDefinitions_ProcessDefinitionVersions_ProcessDef~",
                        column: x => x.ProcessDefinitionVersionId,
                        principalTable: "ProcessDefinitionVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessStepPredecessors",
                columns: table => new
                {
                    ProcessStepDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PredecessorStepId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessStepPredecessors", x => new { x.ProcessStepDefinitionId, x.Id });
                    table.ForeignKey(
                        name: "FK_ProcessStepPredecessors_ProcessStepDefinitions_ProcessStepD~",
                        column: x => x.ProcessStepDefinitionId,
                        principalTable: "ProcessStepDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessDefinitions_CountryId_AuthorityId_ApplicationTypeId",
                table: "ProcessDefinitions",
                columns: new[] { "CountryId", "AuthorityId", "ApplicationTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessDefinitions_TenantId_Code",
                table: "ProcessDefinitions",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessDefinitionVersions_ProcessDefinitionId_Status",
                table: "ProcessDefinitionVersions",
                columns: new[] { "ProcessDefinitionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessDefinitionVersions_ProcessDefinitionId_VersionNumber",
                table: "ProcessDefinitionVersions",
                columns: new[] { "ProcessDefinitionId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessStepDefinitions_ParentStepId",
                table: "ProcessStepDefinitions",
                column: "ParentStepId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessStepDefinitions_ProcessDefinitionVersionId_Code",
                table: "ProcessStepDefinitions",
                columns: new[] { "ProcessDefinitionVersionId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessStepPredecessors_ProcessStepDefinitionId_Predecessor~",
                table: "ProcessStepPredecessors",
                columns: new[] { "ProcessStepDefinitionId", "PredecessorStepId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessStepPredecessors");

            migrationBuilder.DropTable(
                name: "ProcessStepDefinitions");

            migrationBuilder.DropTable(
                name: "ProcessDefinitionVersions");

            migrationBuilder.DropTable(
                name: "ProcessDefinitions");
        }
    }
}
