using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <summary>
    /// EPIC-010c S002 — which sites a licence approves, and from when.
    /// </summary>
    /// <remarks>
    /// <b>One table, no backfill, no seed.</b> RegOS holds no record of which
    /// sites any licence approves, so there is nothing to migrate — and
    /// inventing approvals would be worse than an empty table: an approval is
    /// the fact the whole epic compares against, and a fabricated one would make
    /// a divergence disappear rather than merely be unknown.
    /// <para>
    /// <b>The second <em>licence + thing + date</em> table</b>, after
    /// <c>PackAuthorisations</c>, and deliberately a second one rather than a
    /// generalisation of it (ADR-018: two is a pattern, three is when to
    /// evaluate). The foreign keys are asymmetric for the reason
    /// <c>ManufacturingOperations</c>' are: <b>cascade</b> from the licence,
    /// because an approval is a statement it makes; <b>restrict</b> from the
    /// site, because deleting a plant out from under an approval would erase
    /// which sites a filed licence named.
    /// </para>
    /// </remarks>
    public partial class AddSiteApprovals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteApprovals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationSiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    RecordedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteApprovals_OrganizationSites_OrganizationSiteId",
                        column: x => x.OrganizationSiteId,
                        principalTable: "OrganizationSites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SiteApprovals_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SiteApprovals_OrganizationSiteId",
                table: "SiteApprovals",
                column: "OrganizationSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteApprovals_RegistrationId_OrganizationSiteId",
                table: "SiteApprovals",
                columns: new[] { "RegistrationId", "OrganizationSiteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiteApprovals_TenantId",
                table: "SiteApprovals",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteApprovals");
        }
    }
}
