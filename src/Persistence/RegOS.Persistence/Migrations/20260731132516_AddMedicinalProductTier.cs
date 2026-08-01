using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <summary>
    /// Inserts the market-local tier between the global product and the licence.
    /// </summary>
    /// <remarks>
    /// <b>Additive, not transformative.</b> Every medicinal product this
    /// creates is derived from a (tenant, global product, country) triple that
    /// is already on a registration, and every registration ends up attached to
    /// the one representing exactly the market it was already in. No existing
    /// row changes meaning and nothing is inferred.
    /// <para>
    /// Written by hand. The scaffolded version <em>renamed</em>
    /// <c>GlobalProductId</c> to <c>MedicinalProductId</c> and dropped
    /// <c>CountryId</c> — structurally correct and semantically catastrophic:
    /// every registration would have kept a global product's id in a column
    /// meaning something else, pointing at a row that does not exist.
    /// </para>
    /// <para>
    /// The status date is the earliest business date on the registration's own
    /// history, not today: the market presence existed no later than the first
    /// thing that happened in it. It falls back to <c>CURRENT_DATE</c> only for
    /// a registration with no history at all, which the aggregate cannot
    /// produce.
    /// </para>
    /// </remarks>
    public partial class AddMedicinalProductTier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedicinalProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    GlobalProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StatusDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicinalProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicinalProducts_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicinalProducts_Products_GlobalProductId",
                        column: x => x.GlobalProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicinalProducts_CountryId",
                table: "MedicinalProducts",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicinalProducts_GlobalProductId_CountryId",
                table: "MedicinalProducts",
                columns: new[] { "GlobalProductId", "CountryId" });

            migrationBuilder.CreateIndex(
                name: "IX_MedicinalProducts_TenantId",
                table: "MedicinalProducts",
                column: "TenantId");

            // Nullable for the length of the backfill, then tightened. It
            // cannot be added NOT NULL: there is no default that would be true,
            // because every value has to be computed from a row.
            migrationBuilder.AddColumn<Guid>(
                name: "MedicinalProductId",
                table: "Registrations",
                type: "uuid",
                nullable: true);

            // One statement, so the ids that are inserted are the ids that are
            // assigned: RETURNING carries them straight into the UPDATE and
            // there is no window in which a generated id could go astray.
            migrationBuilder.Sql("""
                WITH markets AS (
                    SELECT DISTINCT
                        r."TenantId", r."GlobalProductId", r."CountryId"
                    FROM "Registrations" r
                ),
                dated AS (
                    SELECT
                        m."TenantId",
                        m."GlobalProductId",
                        m."CountryId",
                        COALESCE(MIN(h."OccurredOn"), CURRENT_DATE) AS "StatusDate"
                    FROM markets m
                    JOIN "Registrations" r
                      ON r."TenantId"        = m."TenantId"
                     AND r."GlobalProductId" = m."GlobalProductId"
                     AND r."CountryId"       = m."CountryId"
                    LEFT JOIN "RegistrationStatusHistory" h
                      ON h."RegistrationId" = r."Id"
                    GROUP BY m."TenantId", m."GlobalProductId", m."CountryId"
                ),
                created AS (
                    INSERT INTO "MedicinalProducts" (
                        "Id", "TenantId", "GlobalProductId",
                        "CountryId", "Status", "StatusDate")
                    SELECT
                        gen_random_uuid(),
                        d."TenantId", d."GlobalProductId",
                        d."CountryId", 'Active', d."StatusDate"
                    FROM dated d
                    RETURNING "Id", "TenantId", "GlobalProductId", "CountryId"
                )
                UPDATE "Registrations" r
                SET "MedicinalProductId" = c."Id"
                FROM created c
                WHERE r."TenantId"        = c."TenantId"
                  AND r."GlobalProductId" = c."GlobalProductId"
                  AND r."CountryId"       = c."CountryId";
                """);

            // If the backfill missed a row, this fails here and loudly rather
            // than leaving a registration attached to nothing.
            migrationBuilder.AlterColumn<Guid>(
                name: "MedicinalProductId",
                table: "Registrations",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_MedicinalProductId_CurrentStatus",
                table: "Registrations",
                columns: new[] { "MedicinalProductId", "CurrentStatus" });

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_MedicinalProducts_MedicinalProductId",
                table: "Registrations",
                column: "MedicinalProductId",
                principalTable: "MedicinalProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Only now that every registration reaches its product and its
            // market through the tier do the direct references come off.
            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Countries_CountryId",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Products_GlobalProductId",
                table: "Registrations");

            migrationBuilder.DropIndex(
                name: "IX_Registrations_CountryId_CurrentStatus",
                table: "Registrations");

            migrationBuilder.DropIndex(
                name: "IX_Registrations_GlobalProductId",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "GlobalProductId",
                table: "Registrations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The reverse is lossless in the same way: both facts are read back
            // off the tier that has been holding them.
            migrationBuilder.AddColumn<Guid>(
                name: "GlobalProductId",
                table: "Registrations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryId",
                table: "Registrations",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Registrations" r
                SET "GlobalProductId" = m."GlobalProductId",
                    "CountryId"       = m."CountryId"
                FROM "MedicinalProducts" m
                WHERE m."Id" = r."MedicinalProductId";
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "GlobalProductId",
                table: "Registrations",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CountryId",
                table: "Registrations",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_CountryId_CurrentStatus",
                table: "Registrations",
                columns: new[] { "CountryId", "CurrentStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_GlobalProductId",
                table: "Registrations",
                column: "GlobalProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Countries_CountryId",
                table: "Registrations",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Products_GlobalProductId",
                table: "Registrations",
                column: "GlobalProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_MedicinalProducts_MedicinalProductId",
                table: "Registrations");

            migrationBuilder.DropIndex(
                name: "IX_Registrations_MedicinalProductId_CurrentStatus",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "MedicinalProductId",
                table: "Registrations");

            migrationBuilder.DropTable(
                name: "MedicinalProducts");
        }
    }
}
