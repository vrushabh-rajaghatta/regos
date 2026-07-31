using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationDivisionsAndIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Acronym",
                table: "Organizations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameNativeLanguage",
                table: "Organizations",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "StatusDate",
                table: "Organizations",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.CreateTable(
                name: "OrganizationDivisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Acronym = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StatusDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationDivisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationDivisions_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationIdentifiers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchemeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationIdentifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationIdentifiers_IdentifierSchemes_SchemeId",
                        column: x => x.SchemeId,
                        principalTable: "IdentifierSchemes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationIdentifiers_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationDivisions_OrganizationId",
                table: "OrganizationDivisions",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationDivisions_TenantId",
                table: "OrganizationDivisions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationIdentifiers_OrganizationId_SchemeId",
                table: "OrganizationIdentifiers",
                columns: new[] { "OrganizationId", "SchemeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationIdentifiers_SchemeId",
                table: "OrganizationIdentifiers",
                column: "SchemeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizationDivisions");

            migrationBuilder.DropTable(
                name: "OrganizationIdentifiers");

            migrationBuilder.DropColumn(
                name: "Acronym",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "NameNativeLanguage",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "StatusDate",
                table: "Organizations");
        }
    }
}
