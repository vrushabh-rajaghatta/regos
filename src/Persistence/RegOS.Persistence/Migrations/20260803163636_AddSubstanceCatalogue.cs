using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubstanceCatalogue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Substances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Inn = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    SubstanceClassSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SubstanceClassCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SubstanceClassDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    SubstanceTypeSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SubstanceTypeCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SubstanceTypeDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    CasNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UniiCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MolecularFormula = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Substances", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Substances_TenantId_Name",
                table: "Substances",
                columns: new[] { "TenantId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Substances");
        }
    }
}
