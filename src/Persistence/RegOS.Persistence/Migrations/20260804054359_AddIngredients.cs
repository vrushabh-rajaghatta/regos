using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIngredients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ingredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StrengthNumeratorValue = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    StrengthNumeratorUnitSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StrengthNumeratorUnitCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StrengthNumeratorUnitDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    StrengthDenominatorValue = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    StrengthDenominatorUnitSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StrengthDenominatorUnitCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StrengthDenominatorUnitDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    PharmaceuticalProductDetailId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ingredients_PharmaceuticalProductDetails_PharmaceuticalProd~",
                        column: x => x.PharmaceuticalProductDetailId,
                        principalTable: "PharmaceuticalProductDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ingredients_Substances_SubstanceId",
                        column: x => x.SubstanceId,
                        principalTable: "Substances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_PharmaceuticalProductDetailId_SubstanceId",
                table: "Ingredients",
                columns: new[] { "PharmaceuticalProductDetailId", "SubstanceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_SubstanceId",
                table: "Ingredients",
                column: "SubstanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ingredients");
        }
    }
}
