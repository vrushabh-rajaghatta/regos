using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicinalProductComponents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedicinalProductComponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicinalProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentComponentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ComponentTypeSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ComponentTypeCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ComponentTypeDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    UnitOfPresentationSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UnitOfPresentationCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UnitOfPresentationDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    DoseFormSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DoseFormCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DoseFormDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicinalProductComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicinalProductComponents_MedicinalProductComponents_Paren~",
                        column: x => x.ParentComponentId,
                        principalTable: "MedicinalProductComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicinalProductComponents_MedicinalProducts_MedicinalProdu~",
                        column: x => x.MedicinalProductId,
                        principalTable: "MedicinalProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicinalProductComponents_MedicinalProductId",
                table: "MedicinalProductComponents",
                column: "MedicinalProductId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicinalProductComponents_ParentComponentId",
                table: "MedicinalProductComponents",
                column: "ParentComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicinalProductComponents_TenantId",
                table: "MedicinalProductComponents",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicinalProductComponents");
        }
    }
}
