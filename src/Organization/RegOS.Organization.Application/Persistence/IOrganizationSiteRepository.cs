using RegOS.Organization.Domain.Aggregates.OrganizationSite;

namespace RegOS.Organization.Application.Persistence;

/// <summary>
/// Aggregates only. Reads for screens project from the database directly rather
/// than loading aggregates through here (ADR-006, ADR-016).
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
