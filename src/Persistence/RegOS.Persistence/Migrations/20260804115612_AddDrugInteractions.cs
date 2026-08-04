using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDrugInteractions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Interactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicinalProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    InteractionTypeSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InteractionTypeCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    InteractionTypeDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    LabelText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Management = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SeveritySystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SeverityCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SeverityDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interactions_MedicinalProducts_MedicinalProductId",
                        column: x => x.MedicinalProductId,
                        principalTable: "MedicinalProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Interactants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    SubstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    DrugInteractionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interactants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interactants_Interactions_DrugInteractionId",
                        column: x => x.DrugInteractionId,
                        principalTable: "Interactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Interactants_Substances_SubstanceId",
                        column: x => x.SubstanceId,
                        principalTable: "Substances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InteractionPopulations",
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
                    DrugInteractionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractionPopulations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InteractionPopulations_Interactions_DrugInteractionId",
                        column: x => x.DrugInteractionId,
                        principalTable: "Interactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Interactants_DrugInteractionId",
                table: "Interactants",
                column: "DrugInteractionId");

            migrationBuilder.CreateIndex(
                name: "IX_Interactants_SubstanceId",
                table: "Interactants",
                column: "SubstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractionPopulations_DrugInteractionId",
                table: "InteractionPopulations",
                column: "DrugInteractionId");

            migrationBuilder.CreateIndex(
                name: "IX_Interactions_MedicinalProductId",
                table: "Interactions",
                column: "MedicinalProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Interactions_TenantId",
                table: "Interactions",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Interactants");

            migrationBuilder.DropTable(
                name: "InteractionPopulations");

            migrationBuilder.DropTable(
                name: "Interactions");
        }
    }
}
