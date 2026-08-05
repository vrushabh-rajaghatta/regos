using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegOS.Persistence.Migrations
{
    /// <summary>
    /// EPIC-010c S003 — where an ingredient's substance comes from.
    /// </summary>
    /// <remarks>
    /// <b>Nullable, with no default and no backfill, and all three are the same
    /// decision.</b> RegOS holds no provenance for any ingredient ever recorded,
    /// so there is nothing to migrate — and a <c>NOT NULL DEFAULT</c> would be
    /// exactly the lie EPIC-022 S001's scaffolded migration was caught telling.
    /// <b>Absent means "nobody has said", never "unsourced"</b>, and only a
    /// nullable column can say that.
    /// <para>
    /// <b>No foreign key</b>, unlike the two tables this epic added before it.
    /// <c>Ingredient</c> is a child with no <c>TenantId</c>, reachable only
    /// through a filtered root; <c>OrganizationSite</c> is a root in another
    /// context with a filter of its own. A restrict between them would refuse to
    /// deactivate a plant because a formulation once named it, and a cascade
    /// would delete composition when a registry row went. The id is held by
    /// value, the way <c>TenantId</c> is (ADR-031).
    /// </para>
    /// </remarks>
    public partial class AddIngredientManufacturingSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ManufacturingSourceSiteId",
                table: "Ingredients",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ManufacturingSourceSiteId",
                table: "Ingredients");
        }
    }
}
