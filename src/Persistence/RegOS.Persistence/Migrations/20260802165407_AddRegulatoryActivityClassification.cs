using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <summary>
    /// EPIC-007a S003 — the regulatory activity. Purely additive: two catalogues
    /// and three nullable columns on <c>Submissions</c>.
    /// </summary>
    /// <remarks>
    /// <b>Nothing is backfilled, and that is the decision this migration makes.</b>
    /// S001's migration derived an application's classification from its earliest
    /// sequence, because the value was there to be recovered. Nothing here is:
    /// sub-type is not derivable from an activity's shape (evidence E13 — FDA's
    /// own example #23 opens an activity with <c>report</c>), so any default
    /// would be an invention that later reads as a fact. Every existing sequence
    /// keeps three nulls, which the CHECK constraint below recognises as a
    /// legitimate state rather than a violation.
    /// <para>
    /// <b>The <c>SubmissionTypes</c> table created here is not the one dropped
    /// earlier in this folder.</b> A table of that name was created in
    /// <c>20260714095053_AddSubmissionTypes</c> and renamed to
    /// <c>ApplicationTypes</c> in <c>20260802130746_MoveApplicationTypeToApplication</c>,
    /// because it enumerated eCTD's <c>application-type</c> under the wrong name
    /// (evidence E11, ADR-050). This is the concept the name always meant, and
    /// ADR-050 §4 reserved it for exactly this.
    /// </para>
    /// <para>
    /// <b><c>Down</c> is lossy, deliberately.</b> Dropping the three columns
    /// discards which activity each sequence belonged to, and there is nowhere
    /// else that fact is written. It is a development convenience, not a
    /// rollback plan.
    /// </para>
    /// </remarks>
    public partial class AddRegulatoryActivityClassification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OriginatingSubmissionId",
                table: "Submissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubmissionSubTypeId",
                table: "Submissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubmissionTypeId",
                table: "Submissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "ApplicationTypes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SubmissionSubTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AuthorityId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionSubTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmissionSubTypes_Authorities_AuthorityId",
                        column: x => x.AuthorityId,
                        principalTable: "Authorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AuthorityId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmissionTypes_Authorities_AuthorityId",
                        column: x => x.AuthorityId,
                        principalTable: "Authorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_OriginatingSubmissionId",
                table: "Submissions",
                column: "OriginatingSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_SubmissionSubTypeId",
                table: "Submissions",
                column: "SubmissionSubTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_SubmissionTypeId",
                table: "Submissions",
                column: "SubmissionTypeId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Submissions_ActivityClassification",
                table: "Submissions",
                sql: "(\"SubmissionSubTypeId\" IS NULL\n     AND \"SubmissionTypeId\" IS NULL\n     AND \"OriginatingSubmissionId\" IS NULL)\nOR (\"SubmissionSubTypeId\" IS NOT NULL\n     AND ((\"SubmissionTypeId\" IS NULL)\n           <> (\"OriginatingSubmissionId\" IS NULL)))");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationTypes_AuthorityId_Token",
                table: "ApplicationTypes",
                columns: new[] { "AuthorityId", "Token" },
                unique: true,
                filter: "\"Token\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionSubTypes_AuthorityId",
                table: "SubmissionSubTypes",
                column: "AuthorityId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionSubTypes_AuthorityId_Token",
                table: "SubmissionSubTypes",
                columns: new[] { "AuthorityId", "Token" },
                unique: true,
                filter: "\"Token\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionSubTypes_Code",
                table: "SubmissionSubTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionTypes_AuthorityId",
                table: "SubmissionTypes",
                column: "AuthorityId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionTypes_AuthorityId_Token",
                table: "SubmissionTypes",
                columns: new[] { "AuthorityId", "Token" },
                unique: true,
                filter: "\"Token\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionTypes_Code",
                table: "SubmissionTypes",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_SubmissionSubTypes_SubmissionSubTypeId",
                table: "Submissions",
                column: "SubmissionSubTypeId",
                principalTable: "SubmissionSubTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_SubmissionTypes_SubmissionTypeId",
                table: "Submissions",
                column: "SubmissionTypeId",
                principalTable: "SubmissionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Submissions_OriginatingSubmissionId",
                table: "Submissions",
                column: "OriginatingSubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_SubmissionSubTypes_SubmissionSubTypeId",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_SubmissionTypes_SubmissionTypeId",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Submissions_OriginatingSubmissionId",
                table: "Submissions");

            migrationBuilder.DropTable(
                name: "SubmissionSubTypes");

            migrationBuilder.DropTable(
                name: "SubmissionTypes");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_OriginatingSubmissionId",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_SubmissionSubTypeId",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_SubmissionTypeId",
                table: "Submissions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Submissions_ActivityClassification",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationTypes_AuthorityId_Token",
                table: "ApplicationTypes");

            migrationBuilder.DropColumn(
                name: "OriginatingSubmissionId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "SubmissionSubTypeId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "SubmissionTypeId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "ApplicationTypes");
        }
    }
}
