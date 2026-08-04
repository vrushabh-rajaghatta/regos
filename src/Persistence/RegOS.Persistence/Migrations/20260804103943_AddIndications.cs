using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIndications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Indications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicinalProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConditionSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConditionCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ConditionDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    LabelText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    CurrentStatus = table.Column<int>(type: "integer", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Indications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Indications_MedicinalProducts_MedicinalProductId",
                        column: x => x.MedicinalProductId,
                        principalTable: "MedicinalProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndicationOtherTherapies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationshipSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RelationshipCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RelationshipDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Therapy = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    IndicationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndicationOtherTherapies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndicationOtherTherapies_Indications_IndicationId",
                        column: x => x.IndicationId,
                        principalTable: "Indications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndicationPopulations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgeLow = table.Column<int>(type: "integer", nullable: true),
                    AgeHigh = table.Column<int>(type: "integer", nullable: true),
                    AgeUnitSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AgeUnitCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AgeUnitDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    GenderSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GenderCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GenderDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    PhysiologicalConditionSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PhysiologicalConditionCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PhysiologicalConditionDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IndicationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndicationPopulations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndicationPopulations_Indications_IndicationId",
                        column: x => x.IndicationId,
                        principalTable: "Indications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndicationStatusHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OccurredOn = table.Column<DateOnly>(type: "date", nullable: false),
                    RecordedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IndicationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndicationStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndicationStatusHistory_Indications_IndicationId",
                        column: x => x.IndicationId,
                        principalTable: "Indications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndicationOtherTherapies_IndicationId",
                table: "IndicationOtherTherapies",
                column: "IndicationId");

            migrationBuilder.CreateIndex(
                name: "IX_IndicationPopulations_IndicationId",
                table: "IndicationPopulations",
                column: "IndicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Indications_ConditionCode",
                table: "Indications",
                column: "ConditionCode");

            migrationBuilder.CreateIndex(
                name: "IX_Indications_CurrentStatus",
                table: "Indications",
                column: "CurrentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Indications_MedicinalProductId",
                table: "Indications",
                column: "MedicinalProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Indications_TenantId",
                table: "Indications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_IndicationStatusHistory_IndicationId_OccurredOn",
                table: "IndicationStatusHistory",
                columns: new[] { "IndicationId", "OccurredOn" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndicationOtherTherapies");

            migrationBuilder.DropTable(
                name: "IndicationPopulations");

            migrationBuilder.DropTable(
                name: "IndicationStatusHistory");

            migrationBuilder.DropTable(
                name: "Indications");
        }
    }
}
