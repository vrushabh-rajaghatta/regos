using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPresentationsAndAtcCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AtcCode",
                table: "MedicinalProducts",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PharmaceuticalProductDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicinalProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DoseFormSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DoseFormCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DoseFormDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    UnitOfPresentationSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UnitOfPresentationCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UnitOfPresentationDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmaceuticalProductDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PharmaceuticalProductDetails_MedicinalProducts_MedicinalPro~",
                        column: x => x.MedicinalProductId,
                        principalTable: "MedicinalProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PharmaceuticalProductRoutes",
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
                    table.PrimaryKey("PK_PharmaceuticalProductRoutes", x => new { x.PharmaceuticalProductDetailId, x.Id });
                    table.ForeignKey(
                        name: "FK_PharmaceuticalProductRoutes_PharmaceuticalProductDetails_Ph~",
                        column: x => x.PharmaceuticalProductDetailId,
                        principalTable: "PharmaceuticalProductDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicinalProducts_AtcCode",
                table: "MedicinalProducts",
                column: "AtcCode");

            migrationBuilder.CreateIndex(
                name: "IX_PharmaceuticalProductDetails_MedicinalProductId",
                table: "PharmaceuticalProductDetails",
                column: "MedicinalProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmaceuticalProductDetails_TenantId",
                table: "PharmaceuticalProductDetails",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmaceuticalProductRoutes_PharmaceuticalProductDetailId_C~",
                table: "PharmaceuticalProductRoutes",
                columns: new[] { "PharmaceuticalProductDetailId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PharmaceuticalProductRoutes");

            migrationBuilder.DropTable(
                name: "PharmaceuticalProductDetails");

            migrationBuilder.DropIndex(
                name: "IX_MedicinalProducts_AtcCode",
                table: "MedicinalProducts");

            migrationBuilder.DropColumn(
                name: "AtcCode",
                table: "MedicinalProducts");
        }
    }
}
