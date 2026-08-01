using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHaCorrespondence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CorrespondenceTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorrespondenceTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HaCorrespondence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrespondenceTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    Subject = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    OccurredOn = table.Column<DateOnly>(type: "date", nullable: false),
                    ResponseDueOn = table.Column<DateOnly>(type: "date", nullable: true),
                    AuthorityReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RegulatoryApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: true),
                    RegistrationId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HaCorrespondence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HaCorrespondence_Authorities_AuthorityId",
                        column: x => x.AuthorityId,
                        principalTable: "Authorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HaCorrespondence_CorrespondenceTypes_CorrespondenceTypeId",
                        column: x => x.CorrespondenceTypeId,
                        principalTable: "CorrespondenceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CorrespondenceTypes_Code",
                table: "CorrespondenceTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HaCorrespondence_AuthorityId",
                table: "HaCorrespondence",
                column: "AuthorityId");

            migrationBuilder.CreateIndex(
                name: "IX_HaCorrespondence_CorrespondenceTypeId",
                table: "HaCorrespondence",
                column: "CorrespondenceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_HaCorrespondence_RegulatoryApplicationId",
                table: "HaCorrespondence",
                column: "RegulatoryApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_HaCorrespondence_TenantId",
                table: "HaCorrespondence",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_HaCorrespondence_TenantId_OccurredOn",
                table: "HaCorrespondence",
                columns: new[] { "TenantId", "OccurredOn" });

            migrationBuilder.CreateIndex(
                name: "IX_HaCorrespondence_TenantId_ResponseDueOn",
                table: "HaCorrespondence",
                columns: new[] { "TenantId", "ResponseDueOn" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HaCorrespondence");

            migrationBuilder.DropTable(
                name: "CorrespondenceTypes");
        }
    }
}
