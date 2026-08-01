using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInspections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceInspectionId",
                table: "Commitments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Inspections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    OrganizationSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScheduledFor = table.Column<DateOnly>(type: "date", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Outcome = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    CurrentStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inspections_Authorities_AuthorityId",
                        column: x => x.AuthorityId,
                        principalTable: "Authorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inspections_OrganizationSites_OrganizationSiteId",
                        column: x => x.OrganizationSiteId,
                        principalTable: "OrganizationSites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InspectionStatusEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OccurredOn = table.Column<DateOnly>(type: "date", nullable: false),
                    RecordedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    InspectionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionStatusEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionStatusEntries_Inspections_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "Inspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_AuthorityId",
                table: "Inspections",
                column: "AuthorityId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_OrganizationSiteId",
                table: "Inspections",
                column: "OrganizationSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_TenantId_CurrentStatus_ScheduledFor",
                table: "Inspections",
                columns: new[] { "TenantId", "CurrentStatus", "ScheduledFor" });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionStatusEntries_InspectionId",
                table: "InspectionStatusEntries",
                column: "InspectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InspectionStatusEntries");

            migrationBuilder.DropTable(
                name: "Inspections");

            migrationBuilder.DropColumn(
                name: "SourceInspectionId",
                table: "Commitments");
        }
    }
}
