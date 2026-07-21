using Microsoft.EntityFrameworkCore;

using RegOS.Platform.Domain.Aggregates.Tenant;

namespace RegOS.Persistence.Initialization.Platform;

public sealed class TenantInitializer : IDataInitializer
{
    private readonly RegOSDbContext _dbContext;

    public TenantInitializer(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.Tenants.AnyAsync(cancellationToken))
        {
            _dbContext.Tenants.AddRange(Tenants.Data);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return;
        }

        await ReconcileAsync(cancellationToken);
    }

    /// <summary>
    /// Returns the demo tenants to their intended state so a developer who has
    /// been experimenting locally gets a known starting point on restart.
    ///
    /// Deliberately updates only rows that already exist. Inserting here would
    /// push demo data into any database that happens to hold real tenants,
    /// which is why the seed path above remains insert-only-when-empty. Same
    /// shape as OrganizationInitializer.
    /// </summary>
    private async Task ReconcileAsync(CancellationToken cancellationToken)
    {
        var intended = Tenants.Data.ToDictionary(x => x.Id);

        var existing = await _dbContext.Tenants
            .Where(x => intended.Keys.Contains(x.Id))
            .ToListAsync(cancellationToken);

        foreach (var tenant in existing)
        {
            var expected = intended[tenant.Id];

            if (tenant.Status == expected.Status
                && tenant.Name == expected.Name)
            {
                continue;
            }

            if (tenant.Status != expected.Status)
            {
                if (expected.Status == TenantStatus.Active)
                    tenant.Activate();
                else
                    tenant.Deactivate();
            }

            tenant.Rename(expected.Name);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
