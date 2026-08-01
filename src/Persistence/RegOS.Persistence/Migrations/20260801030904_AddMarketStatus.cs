using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <summary>
    /// Gives every market a commercial lifecycle: a stored current status and
    /// the append-only history that explains it.
    /// </summary>
    /// <remarks>
    /// The backfill is what makes this honest. <c>CurrentMarketStatus</c>
    /// defaults to <c>Planned</c>, which is correct for every existing row —
    /// none has ever recorded a launch — but a current status with no history
    /// behind it would break the aggregate's core invariant on the very first
    /// read. So each existing market gets exactly the entry
    /// <c>MedicinalProduct.Create</c> would have written, dated its own
    /// <c>StatusDate</c>: the day that market presence began.
    /// <para>
    /// Derived from data already there, like S001's. Nothing is inferred and no
    /// existing row changes meaning.
    /// </para>
    /// </remarks>
    public partial class AddMarketStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MedicinalProducts_CountryId",
                table: "MedicinalProducts");

            migrationBuilder.AddColumn<int>(
                name: "CurrentMarketStatus",
                table: "MedicinalProducts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MarketStatusHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OccurredOn = table.Column<DateOnly>(type: "date", nullable: false),
                    RecordedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MedicinalProductId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketStatusHistory_MedicinalProducts_MedicinalProductId",
                        column: x => x.MedicinalProductId,
                        principalTable: "MedicinalProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicinalProducts_CountryId_CurrentMarketStatus",
                table: "MedicinalProducts",
                columns: new[] { "CountryId", "CurrentMarketStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_MarketStatusHistory_MedicinalProductId",
                table: "MarketStatusHistory",
                column: "MedicinalProductId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketStatusHistory_MedicinalProductId_OccurredOn",
                table: "MarketStatusHistory",
                columns: new[] { "MedicinalProductId", "OccurredOn" });

            // The entry Create would have written, for every market that
            // predates it. OccurredOn is the market's own StatusDate — when it
            // happened in the world; RecordedOnUtc is now — when RegOS learned
            // of it. That the two differ is the point of keeping both, and this
            // migration is exactly the case they exist to describe.
            migrationBuilder.Sql("""
                INSERT INTO "MarketStatusHistory" (
                    "Id", "Status", "OccurredOn",
                    "RecordedOnUtc", "Note", "MedicinalProductId")
                SELECT
                    gen_random_uuid(),
                    0,
                    m."StatusDate",
                    now() AT TIME ZONE 'UTC',
                    'Carried over when market status was introduced.',
                    m."Id"
                FROM "MedicinalProducts" m;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketStatusHistory");

            migrationBuilder.DropIndex(
                name: "IX_MedicinalProducts_CountryId_CurrentMarketStatus",
                table: "MedicinalProducts");

            migrationBuilder.DropColumn(
                name: "CurrentMarketStatus",
                table: "MedicinalProducts");

            migrationBuilder.CreateIndex(
                name: "IX_MedicinalProducts_CountryId",
                table: "MedicinalProducts",
                column: "CountryId");
        }
    }
}
