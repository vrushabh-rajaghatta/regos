using Microsoft.EntityFrameworkCore;

namespace RegOS.Persistence.Initialization.Organization;

/// <summary>
/// Seeds the demo sites — <b>after</b> the reference data they depend on.
/// </summary>
/// <remarks>
/// <b>Its own initializer, and the ordering is the whole reason.</b> A site
/// carries registry identifiers, and an identifier names an
/// <c>IdentifierScheme</c> seeded by <c>IdentifierSchemeDataInitializer</c>,
/// which is registered <em>after</em> <c>OrganizationInitializer</c>. Seeding
/// sites from there threw a foreign-key violation on the first boot of an empty
/// database — found by running one, not by reading the registration list.
/// <para>
/// <b>Guarded on its own table, not on Organizations.</b> Sites arrived long
/// after organizations did, so every existing database has organizations and no
/// sites; a guard on Organizations would mean those databases never get one,
/// which is exactly how the registry came to be empty. The
/// <c>AddManufacturingOperations</c> migration writes the same three rows for
/// databases that never reach this path.
/// </para>
/// </remarks>
public sealed class SiteInitializer : IDataInitializer
{
    private readonly RegOSDbContext _dbContext;

    public SiteInitializer(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        // IgnoreQueryFilters: startup has no tenant, so a filtered read reports
        // an empty table every boot and this would re-insert (ADR-032).
        if (await _dbContext.OrganizationSites
                .IgnoreQueryFilters()
                .AnyAsync(cancellationToken))
        {
            return;
        }

        // The organization these hang off, and the countries they stand in,
        // are seeded by initializers registered before this one. If either is
        // absent this is not a database RegOS seeded, and inventing sites in it
        // would be pushing demo data into somebody's real registry.
        var owner = await _dbContext.Organizations
            .IgnoreQueryFilters()
            .AnyAsync(
                x => x.Id == new RegOS.Organization.Domain.Aggregates
                    .Organization.OrganizationId(
                        OrganizationIds.DemoMarketingAuthorizationHolder),
                cancellationToken);

        if (!owner)
            return;

        _dbContext.OrganizationSites.AddRange(Sites.Data);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
