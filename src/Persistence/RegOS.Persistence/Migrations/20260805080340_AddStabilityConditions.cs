using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <summary>
    /// EPIC-022 S004 — what each market accepts, and what each pack was tested
    /// at.
    /// </summary>
    /// <remarks>
    /// <b>Two tables and one backfill, because only one side has an
    /// authoritative source.</b> WHO publishes the long-term stability condition
    /// each member state accepts, so the eight countries can be filled in.
    /// <b>Nothing fills in the packs</b>, and that is deliberate: RegOS holds no
    /// stability data for any pack ever recorded, and inventing a
    /// <c>TestedAt</c> would make the database look more complete than it is.
    /// An empty collection is the honest answer and the screen says so in those
    /// words rather than warning about it.
    /// <para>
    /// <b>The country backfill is hand-written</b>, for the reason S001's,
    /// S002's and S003's were: the seeder is insert-if-empty, so an already
    /// seeded database gets nothing unless the migration writes it, and all
    /// eight markets would silently come back accepting no stability data at
    /// all — which reads on screen as <em>"we cannot judge this"</em> for every
    /// pack in the system.
    /// </para>
    /// <para>
    /// <b>Conditions, not climatic zones</b> (EPIC-022 D6). The plan this
    /// migration was predicted by called for a <c>ClimaticZone</c> column
    /// holding <c>IVA</c>/<c>IVB</c>. Reading the source killed it: WHO
    /// publishes the condition and not the zone letter, ICH withdrew Q1F, and
    /// <b>India accepts 30 °C/70% RH — neither Zone IVA (30/65) nor Zone IVB
    /// (30/75)</b>. A zone column would have held RegOS's interpretation of
    /// WHO's data rather than WHO's data
    /// (<c>docs/evidence/EPIC-022/stability-conditions.md</c>).
    /// </para>
    /// </remarks>
    public partial class AddStabilityConditions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CountryStabilityConditions",
                columns: table => new
                {
                    CountryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    System = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Display = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryStabilityConditions", x => new { x.CountryId, x.Id });
                    table.ForeignKey(
                        name: "FK_CountryStabilityConditions_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackagedProductStabilityConditions",
                columns: table => new
                {
                    PackagedProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    System = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Display = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackagedProductStabilityConditions", x => new { x.PackagedProductId, x.Id });
                    table.ForeignKey(
                        name: "FK_PackagedProductStabilityConditions_PackagedProducts_Package~",
                        column: x => x.PackagedProductId,
                        principalTable: "PackagedProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CountryStabilityConditions_CountryId_Code",
                table: "CountryStabilityConditions",
                columns: new[] { "CountryId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackagedProductStabilityConditions_PackagedProductId_Code",
                table: "PackagedProductStabilityConditions",
                columns: new[] { "PackagedProductId", "Code" },
                unique: true);

            // Keyed on the alpha-2 code rather than the seeded ids, so it is
            // correct in any environment where the seed ran, however those rows
            // were created.
            //
            // Read verbatim off "Stability conditions for WHO Member States by
            // Region", update March 2021. Seven of the eight accept either of
            // two conditions — the table's own wording is "25 °C/60% RH or
            // 30 °C/65% RH" — which is why the match is an overlap and not an
            // equality. India accepts one, and a different one.
            migrationBuilder.Sql("""
                INSERT INTO "CountryStabilityConditions" ("CountryId", "System", "Code", "Display")
                SELECT c."Id", 'regos-internal', v.code, v.display
                FROM "Countries" c
                JOIN (VALUES
                    ('US', '25C_60RH', '25 °C / 60% RH'),
                    ('US', '30C_65RH', '30 °C / 65% RH'),
                    ('CA', '25C_60RH', '25 °C / 60% RH'),
                    ('CA', '30C_65RH', '30 °C / 65% RH'),
                    ('GB', '25C_60RH', '25 °C / 60% RH'),
                    ('GB', '30C_65RH', '30 °C / 65% RH'),
                    ('DE', '25C_60RH', '25 °C / 60% RH'),
                    ('DE', '30C_65RH', '30 °C / 65% RH'),
                    ('FR', '25C_60RH', '25 °C / 60% RH'),
                    ('FR', '30C_65RH', '30 °C / 65% RH'),
                    ('JP', '25C_60RH', '25 °C / 60% RH'),
                    ('JP', '30C_65RH', '30 °C / 65% RH'),
                    ('AU', '25C_60RH', '25 °C / 60% RH'),
                    ('AU', '30C_65RH', '30 °C / 65% RH'),
                    -- The row the feature turns on, and the one a careful guess
                    -- gets wrong: 30 °C/70% RH is neither Zone IVA nor IVB.
                    ('IN', '30C_70RH', '30 °C / 70% RH')
                ) AS v(alpha2, code, display) ON c."Code" = v.alpha2;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CountryStabilityConditions");

            migrationBuilder.DropTable(
                name: "PackagedProductStabilityConditions");
        }
    }
}
