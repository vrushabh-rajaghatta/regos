using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContraindicationsAndUndesirableEffects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contraindications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicinalProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConditionSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConditionCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ConditionDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    LabelText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contraindications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contraindications_MedicinalProducts_MedicinalProductId",
                        column: x => x.MedicinalProductId,
                        principalTable: "MedicinalProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UndesirableEffects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicinalProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    EffectSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EffectCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EffectDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    LabelText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    FrequencySystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FrequencyCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FrequencyDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UndesirableEffects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UndesirableEffects_MedicinalProducts_MedicinalProductId",
                        column: x => x.MedicinalProductId,
                        principalTable: "MedicinalProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContraindicationPopulations",
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
                    ContraindicationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContraindicationPopulations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContraindicationPopulations_Contraindications_Contraindicat~",
                        column: x => x.ContraindicationId,
                        principalTable: "Contraindications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UndesirableEffectPopulations",
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
                    UndesirableEffectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UndesirableEffectPopulations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UndesirableEffectPopulations_UndesirableEffects_Undesirable~",
                        column: x => x.UndesirableEffectId,
                        principalTable: "UndesirableEffects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContraindicationPopulations_ContraindicationId",
                table: "ContraindicationPopulations",
                column: "ContraindicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Contraindications_ConditionCode",
                table: "Contraindications",
                column: "ConditionCode");

            migrationBuilder.CreateIndex(
                name: "IX_Contraindications_MedicinalProductId",
                table: "Contraindications",
                column: "MedicinalProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Contraindications_TenantId",
                table: "Contraindications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UndesirableEffectPopulations_UndesirableEffectId",
                table: "UndesirableEffectPopulations",
                column: "UndesirableEffectId");

            migrationBuilder.CreateIndex(
                name: "IX_UndesirableEffects_EffectCode",
                table: "UndesirableEffects",
                column: "EffectCode");

            migrationBuilder.CreateIndex(
                name: "IX_UndesirableEffects_MedicinalProductId",
                table: "UndesirableEffects",
                column: "MedicinalProductId");

            migrationBuilder.CreateIndex(
                name: "IX_UndesirableEffects_TenantId",
                table: "UndesirableEffects",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContraindicationPopulations");

            migrationBuilder.DropTable(
                name: "UndesirableEffectPopulations");

            migrationBuilder.DropTable(
                name: "Contraindications");

            migrationBuilder.DropTable(
                name: "UndesirableEffects");
        }
    }
}
