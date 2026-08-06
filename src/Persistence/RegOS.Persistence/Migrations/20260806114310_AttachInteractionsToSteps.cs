using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AttachInteractionsToSteps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProcessStepId",
                table: "Inspections",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProcessStepId",
                table: "HaMeetings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProcessStepId",
                table: "HaCorrespondence",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProcessStepId",
                table: "Commitments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_ProcessStepId",
                table: "Inspections",
                column: "ProcessStepId");

            migrationBuilder.CreateIndex(
                name: "IX_HaMeetings_ProcessStepId",
                table: "HaMeetings",
                column: "ProcessStepId");

            migrationBuilder.CreateIndex(
                name: "IX_HaCorrespondence_ProcessStepId",
                table: "HaCorrespondence",
                column: "ProcessStepId");

            migrationBuilder.CreateIndex(
                name: "IX_Commitments_ProcessStepId",
                table: "Commitments",
                column: "ProcessStepId");

            migrationBuilder.AddForeignKey(
                name: "FK_Commitments_ProcessSteps_ProcessStepId",
                table: "Commitments",
                column: "ProcessStepId",
                principalTable: "ProcessSteps",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_HaCorrespondence_ProcessSteps_ProcessStepId",
                table: "HaCorrespondence",
                column: "ProcessStepId",
                principalTable: "ProcessSteps",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_HaMeetings_ProcessSteps_ProcessStepId",
                table: "HaMeetings",
                column: "ProcessStepId",
                principalTable: "ProcessSteps",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Inspections_ProcessSteps_ProcessStepId",
                table: "Inspections",
                column: "ProcessStepId",
                principalTable: "ProcessSteps",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Commitments_ProcessSteps_ProcessStepId",
                table: "Commitments");

            migrationBuilder.DropForeignKey(
                name: "FK_HaCorrespondence_ProcessSteps_ProcessStepId",
                table: "HaCorrespondence");

            migrationBuilder.DropForeignKey(
                name: "FK_HaMeetings_ProcessSteps_ProcessStepId",
                table: "HaMeetings");

            migrationBuilder.DropForeignKey(
                name: "FK_Inspections_ProcessSteps_ProcessStepId",
                table: "Inspections");

            migrationBuilder.DropIndex(
                name: "IX_Inspections_ProcessStepId",
                table: "Inspections");

            migrationBuilder.DropIndex(
                name: "IX_HaMeetings_ProcessStepId",
                table: "HaMeetings");

            migrationBuilder.DropIndex(
                name: "IX_HaCorrespondence_ProcessStepId",
                table: "HaCorrespondence");

            migrationBuilder.DropIndex(
                name: "IX_Commitments_ProcessStepId",
                table: "Commitments");

            migrationBuilder.DropColumn(
                name: "ProcessStepId",
                table: "Inspections");

            migrationBuilder.DropColumn(
                name: "ProcessStepId",
                table: "HaMeetings");

            migrationBuilder.DropColumn(
                name: "ProcessStepId",
                table: "HaCorrespondence");

            migrationBuilder.DropColumn(
                name: "ProcessStepId",
                table: "Commitments");
        }
    }
}
