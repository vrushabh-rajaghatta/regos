using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <summary>
    /// EPIC-010c S001 — where the work happens.
    /// </summary>
    /// <remarks>
    /// <b>One table, no backfill, and that is the honest position.</b> RegOS
    /// holds no record of where anything is made, so there is nothing to
    /// migrate — unlike every migration in EPIC-022, where the seeder being
    /// insert-if-empty meant an existing database had to be written to by hand.
    /// <para>
    /// <b>The first table joining Product to Organization</b>
    /// (<c>docs/adr/ADR-063-where-a-product-is-made-is-a-product-fact.md</c>).
    /// The foreign keys are deliberately asymmetric: <b>cascade</b> from the
    /// market, because an operation is a statement about a product and means
    /// nothing once it is gone; <b>restrict</b> from the site, because deleting
    /// a plant out from under a recorded operation would erase where a filed
    /// product was made. Sites deactivate rather than delete (ES-018), so the
    /// restrict should never fire — which is when a guard is worth having.
    /// </para>
    /// </remarks>
    public partial class AddManufacturingOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ManufacturingOperations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicinalProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationSiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OperationCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OperationDisplay = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    CeasedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    RecordedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManufacturingOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManufacturingOperations_MedicinalProducts_MedicinalProductId",
                        column: x => x.MedicinalProductId,
                        principalTable: "MedicinalProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ManufacturingOperations_OrganizationSites_OrganizationSiteId",
                        column: x => x.OrganizationSiteId,
                        principalTable: "OrganizationSites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingOperations_MedicinalProductId",
                table: "ManufacturingOperations",
                column: "MedicinalProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingOperations_MedicinalProductId_OrganizationSite~",
                table: "ManufacturingOperations",
                columns: new[] { "MedicinalProductId", "OrganizationSiteId" });

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingOperations_OrganizationSiteId",
                table: "ManufacturingOperations",
                column: "OrganizationSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingOperations_TenantId",
                table: "ManufacturingOperations",
                column: "TenantId");

            // One *open* period per (market, site, operation).
            //
            // ┌──────────────────────────────────────────────────────────────┐
            // │ THIS INDEX IS INTENTIONALLY MAINTAINED BY THIS MIGRATION.    │
            // │                                                              │
            // │ It cannot currently be expressed through EF, because it      │
            // │ spans an owned value's column and the owner's columns. It is │
            // │ NOT an accidental omission from the configuration, and it is │
            // │ NOT safe to assume a scaffolded migration will recreate it.  │
            // └──────────────────────────────────────────────────────────────┘
            //
            // The detail behind that:
            // "OperationCode" belongs to the owned `Operation` value rather
            // than to the entity, and EF resolves HasIndex property names
            // against the owner alone. Both columns are in this table, so the
            // index is ordinary SQL — it is simply not reachable from the
            // model, and is therefore absent from the snapshot. EF only diffs
            // what it knows about and will never drop it; the configuration
            // carries a note pointing here, because a fresh database gets this
            // index from this file and from nowhere else.
            //
            // Filtered on CeasedOn IS NULL, because the same site performing
            // the same operation over two *closed* periods is ordinary — a
            // transfer away and back is exactly that, and the closed rows are
            // what keep a 2023 filing explainable.
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX "IX_ManufacturingOperations_CurrentPerSiteOperation"
                ON "ManufacturingOperations"
                    ("MedicinalProductId", "OrganizationSiteId", "OperationCode")
                WHERE "CeasedOn" IS NULL;
                """);

            // Three demo sites, and the reason they are here is the finding
            // itself: **the site registry was empty.**
            //
            // OrganizationSite has been an aggregate root since EPIC-016 and
            // nothing ever seeded one, so "which sites make this product?" had
            // no answer it could give and the site picker offered nothing to
            // pick. The browser proof found it, not review — the same way
            // EPIC-022 S002 found that neither EU market had an authority.
            //
            // Written here as well as in the seeder for the reason every
            // EPIC-022 backfill was: the seeder is insert-if-empty, so a
            // database that already holds organizations gets nothing from it.
            //
            // **Joined rather than asserted**, which this insert learned the
            // hard way: a database built by `database update` alone has no
            // organizations and no countries, so an unguarded insert fails on a
            // foreign key at migration time. Joining means it writes nothing on
            // a bare schema and the seeder does the work at startup instead.
            //
            // **And guarded on the table being empty**, which it learned the
            // harder way — a developer database was found holding 17 real
            // sites, and an insert keyed only on the demo ids would have
            // dropped three invented facilities into somebody's live registry.
            // This is the rule OrganizationInitializer already states in as
            // many words: *"inserting here would push demo data into any
            // database that happens to hold real organizations, which is why
            // the seed path remains insert-only-when-empty."* A migration is
            // not exempt from it.
            //
            // Invented facilities at invented addresses with invented registry
            // numbers. No FEI or DUNS below is issued.
            migrationBuilder.Sql("""
                INSERT INTO "OrganizationSites"
                    ("Id", "TenantId", "OrganizationId", "Name",
                     "NameNativeLanguage", "Type", "CountryId", "AddressLine1",
                     "City", "StateProvince", "PostalCode", "Status", "StatusDate")
                SELECT v.id::uuid, o."TenantId", o."Id",
                       v.name, v.native, v.type, c."Id",
                       v.line1, v.city, v.state, v.postcode, 1, v.since::date
                FROM (VALUES
                    ('40000000-0000-0000-0000-000000000001',
                     'Demo Pharma Werk Köln', 'Demo Pharma Werk Köln', 1, 'DE',
                     'Industriestraße 14', 'Köln', NULL, '50733', '2014-04-01'),
                    ('40000000-0000-0000-0000-000000000002',
                     'Demo Analytical Services', NULL, 3, 'GB',
                     'Unit 7, Trafford Park', 'Manchester', NULL, 'M17 1AB', '2018-09-01'),
                    ('40000000-0000-0000-0000-000000000003',
                     'Demo Active Ingredients Pvt Ltd', NULL, 1, 'IN',
                     'Plot 42, Genome Valley', 'Hyderabad', 'Telangana', '500078', '2021-06-01')
                ) AS v(id, name, native, type, alpha2, line1, city, state, postcode, since)
                JOIN "Countries" c ON c."Code" = v.alpha2
                -- Demo MAH Ltd., which is the tenant dev@regos.local belongs
                -- to. NOT Demo Manufacturer Ltd., whose name is the obvious
                -- choice and whose rows nobody who logs in can see.
                JOIN "Organizations" o
                     ON o."Id" = '30000000-0000-0000-0000-000000000003'::uuid
                WHERE NOT EXISTS (SELECT 1 FROM "OrganizationSites");
                """);

            // What registries know them as — the field a filing quotes, and the
            // reason the manufacturing read joins the site rather than copying
            // its name. An FEI on a German plant is ordinary: FDA issues one to
            // any site supplying the US market, wherever it stands.
            //
            // Joined on both the site and the scheme, so this writes nothing
            // when the insert above wrote nothing.
            migrationBuilder.Sql("""
                INSERT INTO "SiteIdentifiers"
                    ("Id", "SchemeId", "Value", "OrganizationSiteId")
                SELECT v.id::uuid, k."Id", v.value, s."Id"
                FROM (VALUES
                    ('41000000-0000-0000-0000-000000000001', 'FEI', '3009876543',
                     '40000000-0000-0000-0000-000000000001'),
                    ('41000000-0000-0000-0000-000000000002', 'DUNS', '315551234',
                     '40000000-0000-0000-0000-000000000001'),
                    ('41000000-0000-0000-0000-000000000003', 'DUNS', '225557890',
                     '40000000-0000-0000-0000-000000000002'),
                    ('41000000-0000-0000-0000-000000000004', 'FEI', '3012345678',
                     '40000000-0000-0000-0000-000000000003')
                ) AS v(id, scheme, value, site)
                JOIN "OrganizationSites" s ON s."Id" = v.site::uuid
                JOIN "IdentifierSchemes" k ON k."Code" = v.scheme
                WHERE NOT EXISTS (
                    SELECT 1 FROM "SiteIdentifiers" i WHERE i."Id" = v.id::uuid
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """DROP INDEX IF EXISTS "IX_ManufacturingOperations_CurrentPerSiteOperation";""");

            migrationBuilder.DropTable(
                name: "ManufacturingOperations");
        }
    }
}
