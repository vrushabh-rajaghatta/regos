using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCommitments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                table: "HaQuestions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Commitments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DueOn = table.Column<DateOnly>(type: "date", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RegistrationId = table.Column<Guid>(type: "uuid", nullable: true),
                    RegulatoryApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceCorrespondenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commitments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Commitments_Authorities_AuthorityId",
                        column: x => x.AuthorityId,
                        principalTable: "Authorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommitmentStatusEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OccurredOn = table.Column<DateOnly>(type: "date", nullable: false),
                    RecordedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CommitmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitmentStatusEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommitmentStatusEntries_Commitments_CommitmentId",
                        column: x => x.CommitmentId,
                        principalTable: "Commitments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HaQuestions_CurrentStatus_TargetResponseOn",
                table: "HaQuestions",
                columns: new[] { "CurrentStatus", "TargetResponseOn" });

            migrationBuilder.CreateIndex(
                name: "IX_Commitments_AuthorityId",
                table: "Commitments",
                column: "AuthorityId");

            migrationBuilder.CreateIndex(
                name: "IX_Commitments_TenantId_CurrentStatus_DueOn",
                table: "Commitments",
                columns: new[] { "TenantId", "CurrentStatus", "DueOn" });

            migrationBuilder.CreateIndex(
                name: "IX_Commitments_TenantId_OwnerUserId",
                table: "Commitments",
                columns: new[] { "TenantId", "OwnerUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_CommitmentStatusEntries_CommitmentId",
                table: "CommitmentStatusEntries",
                column: "CommitmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommitmentStatusEntries");

            migrationBuilder.DropTable(
                name: "Commitments");

            migrationBuilder.DropIndex(
                name: "IX_HaQuestions_CurrentStatus_TargetResponseOn",
                table: "HaQuestions");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "HaQuestions");
        }
    }
}
