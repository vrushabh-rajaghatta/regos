using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationSites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IdentifierSchemes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Issuer = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentifierSchemes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationSites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    NameNativeLanguage = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddressLine1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AddressLine2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AddressLine3 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    StateProvince = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StatusDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationSites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationSites_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationSites_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SiteIdentifiers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchemeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OrganizationSiteId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteIdentifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteIdentifiers_IdentifierSchemes_SchemeId",
                        column: x => x.SchemeId,
                        principalTable: "IdentifierSchemes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SiteIdentifiers_OrganizationSites_OrganizationSiteId",
                        column: x => x.OrganizationSiteId,
                        principalTable: "OrganizationSites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IdentifierSchemes_Code",
                table: "IdentifierSchemes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationSites_CountryId",
                table: "OrganizationSites",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationSites_Name",
                table: "OrganizationSites",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationSites_OrganizationId",
                table: "OrganizationSites",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationSites_TenantId",
                table: "OrganizationSites",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationSites_TenantId_Type",
                table: "OrganizationSites",
                columns: new[] { "TenantId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_SiteIdentifiers_OrganizationSiteId_SchemeId",
                table: "SiteIdentifiers",
                columns: new[] { "OrganizationSiteId", "SchemeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiteIdentifiers_SchemeId",
                table: "SiteIdentifiers",
                column: "SchemeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteIdentifiers");

            migrationBuilder.DropTable(
                name: "IdentifierSchemes");

            migrationBuilder.DropTable(
                name: "OrganizationSites");
        }
    }
}
