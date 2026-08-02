using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <summary>
    /// EPIC-007a S001 — the application classification moves from the sequence
    /// to the application (evidence E11).
    /// </summary>
    /// <remarks>
    /// <b>Hand-written, deliberately.</b> The scaffolded version dropped and
    /// recreated <c>SubmissionTypes</c>, which would have deleted every seeded
    /// application type and orphaned the blueprint bindings pointing at them,
    /// and it added <c>RegulatoryApplications.ApplicationTypeId</c> with an
    /// all-zeros default — a value that satisfies NOT NULL while meaning
    /// nothing. Both are replaced below: a rename that keeps the rows, and a
    /// backfill that refuses to invent data.
    /// </remarks>
    public partial class MoveApplicationTypeToApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---- 1. SubmissionTypes becomes ApplicationTypes, rows intact ---
            migrationBuilder.DropForeignKey(
                name: "FK_RegulatoryTemplates_SubmissionTypes_SubmissionTypeId",
                table: "RegulatoryTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_SubmissionTypes_SubmissionTypeId",
                table: "Submissions");

            migrationBuilder.RenameTable(
                name: "SubmissionTypes",
                newName: "ApplicationTypes");

            // Postgres renames neither constraints nor indexes with the table.
            migrationBuilder.Sql(
                @"ALTER TABLE ""ApplicationTypes"" RENAME CONSTRAINT ""PK_SubmissionTypes"" TO ""PK_ApplicationTypes"";");

            migrationBuilder.Sql(
                @"ALTER TABLE ""ApplicationTypes"" RENAME CONSTRAINT ""FK_SubmissionTypes_Authorities_AuthorityId"" TO ""FK_ApplicationTypes_Authorities_AuthorityId"";");

            migrationBuilder.RenameIndex(
                name: "IX_SubmissionTypes_AuthorityId",
                table: "ApplicationTypes",
                newName: "IX_ApplicationTypes_AuthorityId");

            migrationBuilder.RenameIndex(
                name: "IX_SubmissionTypes_Code",
                table: "ApplicationTypes",
                newName: "IX_ApplicationTypes_Code");

            // ---- 2. A blueprint binds to an application type ---------------
            migrationBuilder.RenameColumn(
                name: "SubmissionTypeId",
                table: "RegulatoryTemplates",
                newName: "ApplicationTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_RegulatoryTemplates_SubmissionTypeId",
                table: "RegulatoryTemplates",
                newName: "IX_RegulatoryTemplates_ApplicationTypeId");

            // ---- 3. The classification moves up a tier ---------------------
            // Nullable first: there is nothing to put in it until the backfill.
            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationTypeId",
                table: "RegulatoryApplications",
                type: "uuid",
                nullable: true);

            // Every submission in an application already carried the same
            // value — that it did is exactly the defect E11 records — so the
            // earliest is as good as any. DISTINCT ON makes that choice
            // explicit rather than incidental.
            migrationBuilder.Sql(@"
                UPDATE ""RegulatoryApplications"" a
                SET ""ApplicationTypeId"" = s.""SubmissionTypeId""
                FROM (
                    SELECT DISTINCT ON (""ApplicationId"")
                           ""ApplicationId"", ""SubmissionTypeId""
                    FROM ""Submissions""
                    ORDER BY ""ApplicationId"", ""CreatedOn""
                ) s
                WHERE s.""ApplicationId"" = a.""Id"";");

            // An application with no submissions has nothing to infer from, and
            // "unknown application type" is not a state this model admits. Fail
            // here, naming the rows, rather than carry a meaningless value.
            migrationBuilder.Sql(@"
                DO $$
                DECLARE unclassified text;
                BEGIN
                    SELECT string_agg(""Id""::text, ', ')
                    INTO unclassified
                    FROM ""RegulatoryApplications""
                    WHERE ""ApplicationTypeId"" IS NULL;

                    IF unclassified IS NOT NULL THEN
                        RAISE EXCEPTION
                            'Cannot classify these applications: they have no submission to infer an application type from (%). Classify or remove them, then re-run.',
                            unclassified;
                    END IF;
                END $$;");

            migrationBuilder.AlterColumn<Guid>(
                name: "ApplicationTypeId",
                table: "RegulatoryApplications",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryApplications_ApplicationTypeId",
                table: "RegulatoryApplications",
                column: "ApplicationTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_RegulatoryApplications_ApplicationTypes_ApplicationTypeId",
                table: "RegulatoryApplications",
                column: "ApplicationTypeId",
                principalTable: "ApplicationTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegulatoryTemplates_ApplicationTypes_ApplicationTypeId",
                table: "RegulatoryTemplates",
                column: "ApplicationTypeId",
                principalTable: "ApplicationTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // ---- 4. Only now is the sequence's copy redundant --------------
            migrationBuilder.DropIndex(
                name: "IX_Submissions_SubmissionTypeId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "SubmissionTypeId",
                table: "Submissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore the column on Submissions and repopulate it from the
            // application — the direction the data now flows.
            migrationBuilder.AddColumn<Guid>(
                name: "SubmissionTypeId",
                table: "Submissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""Submissions"" s
                SET ""SubmissionTypeId"" = a.""ApplicationTypeId""
                FROM ""RegulatoryApplications"" a
                WHERE a.""Id"" = s.""ApplicationId"";");

            migrationBuilder.AlterColumn<Guid>(
                name: "SubmissionTypeId",
                table: "Submissions",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.DropForeignKey(
                name: "FK_RegulatoryApplications_ApplicationTypes_ApplicationTypeId",
                table: "RegulatoryApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_RegulatoryTemplates_ApplicationTypes_ApplicationTypeId",
                table: "RegulatoryTemplates");

            migrationBuilder.DropIndex(
                name: "IX_RegulatoryApplications_ApplicationTypeId",
                table: "RegulatoryApplications");

            migrationBuilder.DropColumn(
                name: "ApplicationTypeId",
                table: "RegulatoryApplications");

            migrationBuilder.RenameColumn(
                name: "ApplicationTypeId",
                table: "RegulatoryTemplates",
                newName: "SubmissionTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_RegulatoryTemplates_ApplicationTypeId",
                table: "RegulatoryTemplates",
                newName: "IX_RegulatoryTemplates_SubmissionTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationTypes_Code",
                table: "ApplicationTypes",
                newName: "IX_SubmissionTypes_Code");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationTypes_AuthorityId",
                table: "ApplicationTypes",
                newName: "IX_SubmissionTypes_AuthorityId");

            migrationBuilder.Sql(
                @"ALTER TABLE ""ApplicationTypes"" RENAME CONSTRAINT ""FK_ApplicationTypes_Authorities_AuthorityId"" TO ""FK_SubmissionTypes_Authorities_AuthorityId"";");

            migrationBuilder.Sql(
                @"ALTER TABLE ""ApplicationTypes"" RENAME CONSTRAINT ""PK_ApplicationTypes"" TO ""PK_SubmissionTypes"";");

            migrationBuilder.RenameTable(
                name: "ApplicationTypes",
                newName: "SubmissionTypes");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_SubmissionTypeId",
                table: "Submissions",
                column: "SubmissionTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_RegulatoryTemplates_SubmissionTypes_SubmissionTypeId",
                table: "RegulatoryTemplates",
                column: "SubmissionTypeId",
                principalTable: "SubmissionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_SubmissionTypes_SubmissionTypeId",
                table: "Submissions",
                column: "SubmissionTypeId",
                principalTable: "SubmissionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
