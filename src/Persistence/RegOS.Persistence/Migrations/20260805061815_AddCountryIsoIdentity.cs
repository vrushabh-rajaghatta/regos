using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <summary>
    /// EPIC-022 S001 — the two ISO identity fields machine-readable output needs.
    /// </summary>
    /// <remarks>
    /// <b>Hand-written, and the scaffolded version was wrong.</b> EF generated
    /// both columns as <c>NOT NULL DEFAULT ''</c>, which on any database that
    /// has already been seeded gives all eight countries the same empty alpha-3
    /// code — and the unique index below then fails. An empty default is a lie
    /// about data that already exists.
    /// <para>
    /// So: add nullable, backfill the eight from the register, tighten to
    /// <c>NOT NULL</c>, then index. The backfill keys on the <b>alpha-2 code</b>
    /// rather than on the seeded ids, so it is correct in any environment where
    /// the seed ran, however those rows were created.
    /// </para>
    /// <para>
    /// <b>The tightening is the check.</b> If a country exists that this
    /// migration does not name, the <c>NOT NULL</c> alter fails — which is the
    /// right outcome, because nothing but the seeder writes countries today and
    /// a row from anywhere else is worth stopping for.
    /// </para>
    /// </remarks>
    public partial class AddCountryIsoIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IsoAlpha3Code",
                table: "Countries",
                type: "character(3)",
                fixedLength: true,
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IsoName",
                table: "Countries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            // ISO 3166-1, read off the register rather than inferred — neither
            // value is derivable from the alpha-2 code or the common name
            // (docs/evidence/EPIC-022/iso-3166-1.md).
            migrationBuilder.Sql("""
                UPDATE "Countries"
                SET "IsoAlpha3Code" = v.alpha3, "IsoName" = v.iso_name
                FROM (VALUES
                    ('US', 'USA', 'United States of America'),
                    ('CA', 'CAN', 'Canada'),
                    ('GB', 'GBR', 'United Kingdom of Great Britain and Northern Ireland'),
                    ('DE', 'DEU', 'Germany'),
                    ('FR', 'FRA', 'France'),
                    ('JP', 'JPN', 'Japan'),
                    ('AU', 'AUS', 'Australia'),
                    ('IN', 'IND', 'India')
                ) AS v(alpha2, alpha3, iso_name)
                WHERE "Countries"."Code" = v.alpha2;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "IsoAlpha3Code",
                table: "Countries",
                type: "character(3)",
                fixedLength: true,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character(3)",
                oldFixedLength: true,
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IsoName",
                table: "Countries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_IsoAlpha3Code",
                table: "Countries",
                column: "IsoAlpha3Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Countries_IsoAlpha3Code",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "IsoAlpha3Code",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "IsoName",
                table: "Countries");
        }
    }
}
