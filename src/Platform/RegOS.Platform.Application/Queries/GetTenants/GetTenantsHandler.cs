using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.Platform.Application.Queries.GetTenants;

/// <summary>
/// The whole tenant directory, straight from the database (ADR-016). The
/// Tenants table carries no query filter — it is the one global directory
/// (ADR-031) — so no bypass appears here; what makes this a platform-only
/// view is the endpoint's PlatformAdministrator policy, not the query.
/// </summary>
public sealed class GetTenantsHandler
{
    private readonly RegOSDbContext _dbContext;

    public GetTenantsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TenantListItem>> HandleAsync(
        CancellationToken cancellationToken)
    {
        return await _dbContext.Tenants
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new TenantListItem(
                x.Id.Value,
                x.Name,
                x.Status))
            .ToListAsync(cancellationToken);
    }
}
