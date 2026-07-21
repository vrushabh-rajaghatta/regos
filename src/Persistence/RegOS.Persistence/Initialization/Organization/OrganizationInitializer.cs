using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Persistence.Initialization.Organization;

public sealed class OrganizationInitializer : IDataInitializer
{
    private readonly RegOSDbContext _dbContext;

    public OrganizationInitializer(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        // IgnoreQueryFilters: startup has no tenant, so the filter would
        // report an empty table every boot and this would re-insert (ADR-032).
        if (!await _dbContext.Organizations
                .IgnoreQueryFilters()
                .AnyAsync(cancellationToken))
        {
            _dbContext.Organizations.AddRange(Organizations.Data);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return;
        }

        await ReconcileAsync(cancellationToken);
    }

    /// <summary>
    /// Returns the demo organizations to their intended state so a developer
    /// who has been experimenting locally — or a browser spec that mutated
    /// something it did not create — gets a known starting point on restart.
    ///
    /// Deliberately updates only rows that already exist. Inserting here would
    /// push demo data into any database that happens to hold real
    /// organizations, which is why the seed path above remains
    /// insert-only-when-empty.
    /// </summary>
    private async Task ReconcileAsync(CancellationToken cancellationToken)
    {
        var intended = Organizations.Data.ToDictionary(x => x.Id);

        var existing = await _dbContext.Organizations
            .IgnoreQueryFilters()
            .Where(x => intended.Keys.Contains(x.Id))
            .ToListAsync(cancellationToken);

        foreach (var organization in existing)
        {
            var expected = intended[organization.Id];

            if (organization.Status == expected.Status
                && organization.LegalName == expected.LegalName
                && organization.Type == expected.Type)
            {
                continue;
            }

            if (organization.Status != expected.Status)
            {
                if (expected.Status == OrganizationStatus.Active)
                    organization.Activate();
                else
                    organization.Deactivate();
            }

            organization.Rename(expected.LegalName);
            organization.Reclassify(expected.Type);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
