using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyToPlacement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClinicalStudyId",
                table: "SubmissionDocuments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NonClinicalStudyId",
                table: "SubmissionDocuments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionDocuments_ClinicalStudyId",
                table: "SubmissionDocuments",
                column: "ClinicalStudyId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionDocuments_NonClinicalStudyId",
                table: "SubmissionDocuments",
                column: "NonClinicalStudyId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionDocuments_ClinicalStudies_ClinicalStudyId",
                table: "SubmissionDocuments",
                column: "ClinicalStudyId",
                principalTable: "ClinicalStudies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionDocuments_NonClinicalStudies_NonClinicalStudyId",
                table: "SubmissionDocuments",
                column: "NonClinicalStudyId",
                principalTable: "NonClinicalStudies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionDocuments_ClinicalStudies_ClinicalStudyId",
                table: "SubmissionDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionDocuments_NonClinicalStudies_NonClinicalStudyId",
                table: "SubmissionDocuments");

            migrationBuilder.DropIndex(
                name: "IX_SubmissionDocuments_ClinicalStudyId",
                table: "SubmissionDocuments");

            migrationBuilder.DropIndex(
                name: "IX_SubmissionDocuments_NonClinicalStudyId",
                table: "SubmissionDocuments");

            migrationBuilder.DropColumn(
                name: "ClinicalStudyId",
                table: "SubmissionDocuments");

            migrationBuilder.DropColumn(
                name: "NonClinicalStudyId",
                table: "SubmissionDocuments");
        }
    }
}
