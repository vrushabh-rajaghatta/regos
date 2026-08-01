using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthorityDivisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AuthorityDivisionId",
                table: "HaCorrespondence",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuthorityDivisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorityId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorityDivisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorityDivisions_Authorities_AuthorityId",
                        column: x => x.AuthorityId,
                        principalTable: "Authorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HaCorrespondence_AuthorityDivisionId",
                table: "HaCorrespondence",
                column: "AuthorityDivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorityDivisions_AuthorityId_TenantId_Name",
                table: "AuthorityDivisions",
                columns: new[] { "AuthorityId", "TenantId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_HaCorrespondence_AuthorityDivisions_AuthorityDivisionId",
                table: "HaCorrespondence",
                column: "AuthorityDivisionId",
                principalTable: "AuthorityDivisions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HaCorrespondence_AuthorityDivisions_AuthorityDivisionId",
                table: "HaCorrespondence");

            migrationBuilder.DropTable(
                name: "AuthorityDivisions");

            migrationBuilder.DropIndex(
                name: "IX_HaCorrespondence_AuthorityDivisionId",
                table: "HaCorrespondence");

            migrationBuilder.DropColumn(
                name: "AuthorityDivisionId",
                table: "HaCorrespondence");
        }
    }
}
