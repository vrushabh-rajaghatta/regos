using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantToRegulatoryDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "SubmissionSnapshots",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Submissions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "RegulatoryApplications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ProductDocuments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Backfill, hand-written, in dependency order. Each table derives
            // its tenant from its parent — the same rule the handlers now
            // apply at creation (ADR-031):
            //   RegulatoryApplications <- ApplicantOrganizationId (the fused
            //     model made applicant and owner the same party, and AddTenants
            //     turned every organization into a tenant with the same id)
            //   Submissions            <- parent application
            //   SubmissionSnapshots    <- parent submission
            //   ProductDocuments       <- owning product
            // A row whose join finds nothing keeps the all-zero guid, which
            // matches no caller ever — fail closed, and visible in the data
            // rather than silently adopted by some tenant.
            migrationBuilder.Sql(
                """
                UPDATE "RegulatoryApplications"
                SET "TenantId" = "ApplicantOrganizationId";

                UPDATE "Submissions" s
                SET "TenantId" = a."TenantId"
                FROM "RegulatoryApplications" a
                WHERE s."ApplicationId" = a."Id";

                UPDATE "SubmissionSnapshots" ss
                SET "TenantId" = s."TenantId"
                FROM "Submissions" s
                WHERE ss."SubmissionId" = s."Id";

                UPDATE "ProductDocuments" d
                SET "TenantId" = p."TenantId"
                FROM "Products" p
                WHERE d."ProductId" = p."Id";

                ALTER TABLE "RegulatoryApplications" ALTER COLUMN "TenantId" DROP DEFAULT;
                ALTER TABLE "Submissions" ALTER COLUMN "TenantId" DROP DEFAULT;
                ALTER TABLE "SubmissionSnapshots" ALTER COLUMN "TenantId" DROP DEFAULT;
                ALTER TABLE "ProductDocuments" ALTER COLUMN "TenantId" DROP DEFAULT;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionSnapshots_TenantId",
                table: "SubmissionSnapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_TenantId",
                table: "Submissions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryApplications_TenantId",
                table: "RegulatoryApplications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDocuments_TenantId",
                table: "ProductDocuments",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubmissionSnapshots_TenantId",
                table: "SubmissionSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_TenantId",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_RegulatoryApplications_TenantId",
                table: "RegulatoryApplications");

            migrationBuilder.DropIndex(
                name: "IX_ProductDocuments_TenantId",
                table: "ProductDocuments");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SubmissionSnapshots");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "RegulatoryApplications");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ProductDocuments");
        }
    }
}
