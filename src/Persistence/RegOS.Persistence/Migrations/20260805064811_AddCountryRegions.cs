using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <summary>
    /// EPIC-022 S002 — regions in, <c>RegionCode</c> out.
    /// </summary>
    /// <remarks>
    /// <b>EF warned this "may result in the loss of data". It cannot.</b>
    /// <c>RegionCode</c> was defaulted to null by the only factory that set it,
    /// omitted by all eight seeds, had no mutator and no update path — so no
    /// row has ever held a value. Nobody should go looking for lost data,
    /// which is why this is written down here rather than in a commit message.
    /// <para>
    /// RIM says Regions is <b>Multiple</b> anyway, so the single nullable
    /// string could not have held the answer even if something had written to
    /// it: Germany is EU <em>and</em> ICH <em>and</em> PIC/S.
    /// </para>
    /// <para>
    /// <b>The backfill is hand-written</b>, for the reason S001's was: the
    /// seeder is insert-if-empty, so an already-seeded database gets nothing
    /// unless the migration writes it, and every country would silently come
    /// back with no groupings at all.
    /// </para>
    /// <para>
    /// <b>Membership was fetched, not recalled</b>
    /// (<c>docs/evidence/EPIC-022/regional-membership.md</c>). Two rows
    /// contradict what a careful guess produces: <b>Australia and India are ICH
    /// observers rather than members</b>, and India is not a PIC/S participant
    /// — so India is deliberately absent from the insert below, and an empty
    /// collection is its recorded answer.
    /// </para>
    /// </remarks>
    public partial class AddCountryRegions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegionCode",
                table: "Countries");

            migrationBuilder.CreateTable(
                name: "CountryRegions",
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
                    table.PrimaryKey("PK_CountryRegions", x => new { x.CountryId, x.Id });
                    table.ForeignKey(
                        name: "FK_CountryRegions_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CountryRegions_CountryId_Code",
                table: "CountryRegions",
                columns: new[] { "CountryId", "Code" },
                unique: true);

            // Keyed on the alpha-2 code rather than the seeded ids, so it is
            // correct in any environment where the seed ran, however those rows
            // were created. India is absent on purpose.
            migrationBuilder.Sql("""
                INSERT INTO "CountryRegions" ("CountryId", "System", "Code", "Display")
                SELECT c."Id", 'regos-internal', v.code, v.display
                FROM "Countries" c
                JOIN (VALUES
                    ('US', 'ICH',   'ICH'),
                    ('US', 'PIC_S', 'PIC/S'),
                    ('CA', 'ICH',   'ICH'),
                    ('CA', 'PIC_S', 'PIC/S'),
                    ('GB', 'ICH',   'ICH'),
                    ('GB', 'PIC_S', 'PIC/S'),
                    ('DE', 'EU',    'European Union'),
                    ('DE', 'ICH',   'ICH'),
                    ('DE', 'PIC_S', 'PIC/S'),
                    ('FR', 'EU',    'European Union'),
                    ('FR', 'ICH',   'ICH'),
                    ('FR', 'PIC_S', 'PIC/S'),
                    ('JP', 'ICH',   'ICH'),
                    ('JP', 'PIC_S', 'PIC/S'),
                    ('AU', 'PIC_S', 'PIC/S')
                ) AS v(alpha2, code, display) ON c."Code" = v.alpha2;
                """);

            // Neither EU member state had a regulatory authority, so no EU
            // market could hold a registration and "which of our markets are in
            // the EU?" had no demonstrable answer. Found by this story's browser
            // proof rather than by review.
            //
            // The national agencies, not EMA: an Authority hangs off a
            // CountryId, and EMA is the Union's rather than any member state's.
            // Insert-if-absent, because the seeder only runs on an empty table.
            migrationBuilder.Sql("""
                INSERT INTO "Authorities" ("Id", "Code", "Name", "CountryId")
                SELECT v.id::uuid, v.code, v.name, c."Id"
                FROM "Countries" c
                JOIN (VALUES
                    ('20000000-0000-0000-0000-000000000007', 'BfArM',
                     'Bundesinstitut für Arzneimittel und Medizinprodukte', 'DE'),
                    ('20000000-0000-0000-0000-000000000008', 'ANSM',
                     'Agence nationale de sécurité du médicament et des produits de santé', 'FR')
                ) AS v(id, code, name, alpha2) ON c."Code" = v.alpha2
                WHERE NOT EXISTS (
                    SELECT 1 FROM "Authorities" a WHERE a."Id" = v.id::uuid
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CountryRegions");

            migrationBuilder.AddColumn<string>(
                name: "RegionCode",
                table: "Countries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }
    }
}
