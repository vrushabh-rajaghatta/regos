using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.Persistence;

namespace RegOS.Organization.Infrastructure.Persistence;

public sealed class OrganizationSiteRepository : IOrganizationSiteRepository
{
    private readonly RegOSDbContext _dbContext;

    public OrganizationSiteRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        OrganizationSite site,
        CancellationToken cancellationToken)
    {
        await _dbContext.OrganizationSites.AddAsync(site, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Identifiers come with the site: they are part of the aggregate, and a
    /// command that adds one needs the others loaded to enforce one-per-scheme.
    /// </summary>
    public async Task<OrganizationSite?> GetByIdAsync(
        OrganizationSiteId id,
        CancellationToken cancellationToken)
        => await _dbContext.OrganizationSites
            .Include(x => x.Identifiers)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task UpdateAsync(
        OrganizationSite site,
        CancellationToken cancellationToken)
    {
        _dbContext.OrganizationSites.Update(site);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
