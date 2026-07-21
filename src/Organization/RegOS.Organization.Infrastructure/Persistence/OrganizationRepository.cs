using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Application.Persistence;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Persistence;

using OrganizationAggregate =
    RegOS.Organization.Domain.Aggregates.Organization.Organization;

namespace RegOS.Organization.Infrastructure.Persistence;

/// <summary>
/// Saves within each method, matching the convention the Platform and Product
/// repositories established: one DbContext per request already is the unit of
/// work (ADR-017).
/// </summary>
public sealed class OrganizationRepository : IOrganizationRepository
{
    private readonly RegOSDbContext _dbContext;

    public OrganizationRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        OrganizationAggregate organization,
        CancellationToken cancellationToken)
    {
        await _dbContext.Organizations.AddAsync(organization, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<OrganizationAggregate?> GetByIdAsync(
        OrganizationId id,
        CancellationToken cancellationToken)
        => await _dbContext.Organizations
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
