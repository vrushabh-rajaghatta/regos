using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AttachSubmissionsAndRegistrationsToSteps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProcessStepId",
                table: "Submissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProcessStepId",
                table: "Registrations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ProcessStepId",
                table: "Submissions",
                column: "ProcessStepId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_ProcessStepId",
                table: "Registrations",
                column: "ProcessStepId");

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_ProcessSteps_ProcessStepId",
                table: "Registrations",
                column: "ProcessStepId",
                principalTable: "ProcessSteps",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_ProcessSteps_ProcessStepId",
                table: "Submissions",
                column: "ProcessStepId",
                principalTable: "ProcessSteps",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_ProcessSteps_ProcessStepId",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_ProcessSteps_ProcessStepId",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_ProcessStepId",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Registrations_ProcessStepId",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "ProcessStepId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "ProcessStepId",
                table: "Registrations");
        }
    }
}
