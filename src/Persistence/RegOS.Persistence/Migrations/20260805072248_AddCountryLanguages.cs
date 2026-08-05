using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <summary>
    /// EPIC-022 S003 — the languages a market's labelling is expected in.
    /// </summary>
    /// <remarks>
    /// <b>Backfilled by hand</b>, for the reason S001's and S002's were: the
    /// seeder is insert-if-empty, so an already-seeded database would come back
    /// with no expected languages and the advisory check would say every market
    /// was complete — which is worse than saying nothing.
    /// <para>
    /// <b>Canada is the row this story turns on</b>, and the only one with two.
    /// Bilingual mock-ups of the labels, the package insert and the Product
    /// Monograph are required at submission (C.01.014.1(2)(m.1),
    /// C.08.002(2)(j.1)) — see <c>docs/evidence/EPIC-022/label-languages.md</c>.
    /// </para>
    /// </remarks>
    public partial class AddCountryLanguages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CountryLanguages",
                columns: table => new
                {
                    CountryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryLanguages", x => new { x.CountryId, x.Id });
                    table.ForeignKey(
                        name: "FK_CountryLanguages_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CountryLanguages_CountryId_Language",
                table: "CountryLanguages",
                columns: new[] { "CountryId", "Language" },
                unique: true);

            // Keyed on the alpha-2 code rather than the seeded ids, so it is
            // correct in any environment where the seed ran.
            migrationBuilder.Sql("""
                INSERT INTO "CountryLanguages" ("CountryId", "Language")
                SELECT c."Id", v.language
                FROM "Countries" c
                JOIN (VALUES
                    ('US', 'en'),
                    ('CA', 'en'),
                    ('CA', 'fr'),
                    ('GB', 'en'),
                    ('DE', 'de'),
                    ('FR', 'fr'),
                    ('JP', 'ja'),
                    ('AU', 'en'),
                    ('IN', 'en')
                ) AS v(alpha2, language) ON c."Code" = v.alpha2;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CountryLanguages");
        }
    }
}
