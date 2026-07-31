namespace RegOS.Organization.Domain.Aggregates.OrganizationSite;

/// <summary>
/// Aggregates only. Reads for screens project from <c>RegOSDbContext</c>
/// directly with <c>AsNoTracking()</c> — a query handler never loads an
/// aggregate (ADR-016).
/// </summary>
public interface IOrganizationSiteRepository
{
    Task AddAsync(OrganizationSite site, CancellationToken cancellationToken);

    /// <summary>Loads the site with its identifiers — the whole aggregate.</summary>
    Task<OrganizationSite?> GetByIdAsync(
        OrganizationSiteId id,
        CancellationToken cancellationToken);

    Task UpdateAsync(OrganizationSite site, CancellationToken cancellationToken);
}
