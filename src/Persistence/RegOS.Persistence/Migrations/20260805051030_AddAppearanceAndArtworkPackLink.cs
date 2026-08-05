using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAppearanceAndArtworkPackLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppearanceDescription",
                table: "PharmaceuticalProductDetails",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppearanceImprint",
                table: "PharmaceuticalProductDetails",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppearanceShapeCode",
                table: "PharmaceuticalProductDetails",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppearanceShapeDisplay",
                table: "PharmaceuticalProductDetails",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppearanceShapeSystem",
                table: "PharmaceuticalProductDetails",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PackagedProductId",
                table: "LocalLabels",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PharmaceuticalProductColours",
                columns: table => new
                {
                    PharmaceuticalProductDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    System = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Display = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmaceuticalProductColours", x => new { x.PharmaceuticalProductDetailId, x.Id });
                    table.ForeignKey(
                        name: "FK_PharmaceuticalProductColours_PharmaceuticalProductDetails_P~",
                        column: x => x.PharmaceuticalProductDetailId,
                        principalTable: "PharmaceuticalProductDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocalLabels_PackagedProductId",
                table: "LocalLabels",
                column: "PackagedProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmaceuticalProductColours_PharmaceuticalProductDetailId_~",
                table: "PharmaceuticalProductColours",
                columns: new[] { "PharmaceuticalProductDetailId", "Code" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LocalLabels_PackagedProducts_PackagedProductId",
                table: "LocalLabels",
                column: "PackagedProductId",
                principalTable: "PackagedProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocalLabels_PackagedProducts_PackagedProductId",
                table: "LocalLabels");

            migrationBuilder.DropTable(
                name: "PharmaceuticalProductColours");

            migrationBuilder.DropIndex(
                name: "IX_LocalLabels_PackagedProductId",
                table: "LocalLabels");

            migrationBuilder.DropColumn(
                name: "AppearanceDescription",
                table: "PharmaceuticalProductDetails");

            migrationBuilder.DropColumn(
                name: "AppearanceImprint",
                table: "PharmaceuticalProductDetails");

            migrationBuilder.DropColumn(
                name: "AppearanceShapeCode",
                table: "PharmaceuticalProductDetails");

            migrationBuilder.DropColumn(
                name: "AppearanceShapeDisplay",
                table: "PharmaceuticalProductDetails");

            migrationBuilder.DropColumn(
                name: "AppearanceShapeSystem",
                table: "PharmaceuticalProductDetails");

            migrationBuilder.DropColumn(
                name: "PackagedProductId",
                table: "LocalLabels");
        }
    }
}
