using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Registrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorityId = table.Column<Guid>(type: "uuid", nullable: false),
                    HolderOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginatingApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    RegistrationNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CurrentStatus = table.Column<int>(type: "integer", nullable: false),
                    ApprovedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpiresOn = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Registrations_Authorities_AuthorityId",
                        column: x => x.AuthorityId,
                        principalTable: "Authorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Registrations_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Registrations_Organizations_HolderOrganizationId",
                        column: x => x.HolderOrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Registrations_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Registrations_RegulatoryApplications_OriginatingApplication~",
                        column: x => x.OriginatingApplicationId,
                        principalTable: "RegulatoryApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationStatusHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OccurredOn = table.Column<DateOnly>(type: "date", nullable: false),
                    RecordedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RegistrationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrationStatusHistory_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_AuthorityId",
                table: "Registrations",
                column: "AuthorityId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_CountryId_CurrentStatus",
                table: "Registrations",
                columns: new[] { "CountryId", "CurrentStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_HolderOrganizationId",
                table: "Registrations",
                column: "HolderOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_OriginatingApplicationId",
                table: "Registrations",
                column: "OriginatingApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_ProductId",
                table: "Registrations",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_TenantId",
                table: "Registrations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationStatusHistory_RegistrationId",
                table: "RegistrationStatusHistory",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationStatusHistory_RegistrationId_OccurredOn",
                table: "RegistrationStatusHistory",
                columns: new[] { "RegistrationId", "OccurredOn" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrationStatusHistory");

            migrationBuilder.DropTable(
                name: "Registrations");
        }
    }
}
