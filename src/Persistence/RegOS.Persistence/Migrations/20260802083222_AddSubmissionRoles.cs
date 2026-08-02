using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubmissionRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmissionRoles_ContactRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "ContactRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubmissionRoles_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubmissionRoles_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionRoles_ContactId",
                table: "SubmissionRoles",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionRoles_RoleId",
                table: "SubmissionRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionRoles_SubmissionId",
                table: "SubmissionRoles",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionRoles_SubmissionId_ContactId_RoleId",
                table: "SubmissionRoles",
                columns: new[] { "SubmissionId", "ContactId", "RoleId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubmissionRoles");
        }
    }
}
