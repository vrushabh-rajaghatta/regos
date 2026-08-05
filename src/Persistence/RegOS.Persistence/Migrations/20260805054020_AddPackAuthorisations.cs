using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPackAuthorisations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PackAuthorisations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackagedProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorisedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    RecordedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackAuthorisations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackAuthorisations_PackagedProducts_PackagedProductId",
                        column: x => x.PackagedProductId,
                        principalTable: "PackagedProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PackAuthorisations_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PackAuthorisations_PackagedProductId",
                table: "PackAuthorisations",
                column: "PackagedProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PackAuthorisations_RegistrationId_PackagedProductId",
                table: "PackAuthorisations",
                columns: new[] { "RegistrationId", "PackagedProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackAuthorisations_TenantId",
                table: "PackAuthorisations",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackAuthorisations");
        }
    }
}
