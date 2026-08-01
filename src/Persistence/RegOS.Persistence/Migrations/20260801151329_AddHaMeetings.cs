using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHaMeetings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceMeetingId",
                table: "Commitments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HaMeetings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Subject = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    AuthorityDivisionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScheduledFor = table.Column<DateOnly>(type: "date", nullable: true),
                    RegulatoryApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Minutes = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    Outcome = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CurrentStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HaMeetings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HaMeetings_Authorities_AuthorityId",
                        column: x => x.AuthorityId,
                        principalTable: "Authorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HaMeetings_AuthorityDivisions_AuthorityDivisionId",
                        column: x => x.AuthorityDivisionId,
                        principalTable: "AuthorityDivisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HaMeetingStatusEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OccurredOn = table.Column<DateOnly>(type: "date", nullable: false),
                    RecordedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HaMeetingId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HaMeetingStatusEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HaMeetingStatusEntries_HaMeetings_HaMeetingId",
                        column: x => x.HaMeetingId,
                        principalTable: "HaMeetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HaMeetings_AuthorityDivisionId",
                table: "HaMeetings",
                column: "AuthorityDivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_HaMeetings_AuthorityId",
                table: "HaMeetings",
                column: "AuthorityId");

            migrationBuilder.CreateIndex(
                name: "IX_HaMeetings_TenantId_CurrentStatus_ScheduledFor",
                table: "HaMeetings",
                columns: new[] { "TenantId", "CurrentStatus", "ScheduledFor" });

            migrationBuilder.CreateIndex(
                name: "IX_HaMeetingStatusEntries_HaMeetingId",
                table: "HaMeetingStatusEntries",
                column: "HaMeetingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HaMeetingStatusEntries");

            migrationBuilder.DropTable(
                name: "HaMeetings");

            migrationBuilder.DropColumn(
                name: "SourceMeetingId",
                table: "Commitments");
        }
    }
}
