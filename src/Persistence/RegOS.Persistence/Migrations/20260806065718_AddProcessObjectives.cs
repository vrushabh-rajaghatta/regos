using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessObjectives : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessObjectives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    GlobalProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicinalProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    RegulatoryApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Rationale = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetCompletionOn = table.Column<DateOnly>(type: "date", nullable: true),
                    CurrentStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessObjectives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessObjectives_MedicinalProducts_MedicinalProductId",
                        column: x => x.MedicinalProductId,
                        principalTable: "MedicinalProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessObjectives_Products_GlobalProductId",
                        column: x => x.GlobalProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProcessObjectiveStatusHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OccurredOn = table.Column<DateOnly>(type: "date", nullable: false),
                    RecordedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ProcessObjectiveId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessObjectiveStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessObjectiveStatusHistory_ProcessObjectives_ProcessObje~",
                        column: x => x.ProcessObjectiveId,
                        principalTable: "ProcessObjectives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessObjectives_GlobalProductId_CountryId",
                table: "ProcessObjectives",
                columns: new[] { "GlobalProductId", "CountryId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessObjectives_MedicinalProductId",
                table: "ProcessObjectives",
                column: "MedicinalProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessObjectives_TenantId_CurrentStatus",
                table: "ProcessObjectives",
                columns: new[] { "TenantId", "CurrentStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessObjectiveStatusHistory_ProcessObjectiveId",
                table: "ProcessObjectiveStatusHistory",
                column: "ProcessObjectiveId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessObjectiveStatusHistory");

            migrationBuilder.DropTable(
                name: "ProcessObjectives");
        }
    }
}
