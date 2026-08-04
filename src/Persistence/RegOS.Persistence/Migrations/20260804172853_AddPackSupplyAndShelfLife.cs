using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPackSupplyAndShelfLife : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LegalStatusOfSupplyCode",
                table: "PackagedProducts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalStatusOfSupplyDisplay",
                table: "PackagedProducts",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalStatusOfSupplySystem",
                table: "PackagedProducts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShelfLifeText",
                table: "PackagedProducts",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShelfLifeUnitCode",
                table: "PackagedProducts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShelfLifeUnitDisplay",
                table: "PackagedProducts",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShelfLifeUnitSystem",
                table: "PackagedProducts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShelfLifeValue",
                table: "PackagedProducts",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PackagedProductStorageConditions",
                columns: table => new
                {
                    PackagedProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    System = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Display = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackagedProductStorageConditions", x => new { x.PackagedProductId, x.Id });
                    table.ForeignKey(
                        name: "FK_PackagedProductStorageConditions_PackagedProducts_PackagedP~",
                        column: x => x.PackagedProductId,
                        principalTable: "PackagedProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PackagedProductStorageConditions_PackagedProductId_Code",
                table: "PackagedProductStorageConditions",
                columns: new[] { "PackagedProductId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackagedProductStorageConditions");

            migrationBuilder.DropColumn(
                name: "LegalStatusOfSupplyCode",
                table: "PackagedProducts");

            migrationBuilder.DropColumn(
                name: "LegalStatusOfSupplyDisplay",
                table: "PackagedProducts");

            migrationBuilder.DropColumn(
                name: "LegalStatusOfSupplySystem",
                table: "PackagedProducts");

            migrationBuilder.DropColumn(
                name: "ShelfLifeText",
                table: "PackagedProducts");

            migrationBuilder.DropColumn(
                name: "ShelfLifeUnitCode",
                table: "PackagedProducts");

            migrationBuilder.DropColumn(
                name: "ShelfLifeUnitDisplay",
                table: "PackagedProducts");

            migrationBuilder.DropColumn(
                name: "ShelfLifeUnitSystem",
                table: "PackagedProducts");

            migrationBuilder.DropColumn(
                name: "ShelfLifeValue",
                table: "PackagedProducts");
        }
    }
}
