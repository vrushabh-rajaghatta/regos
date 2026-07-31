using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Domain.Aggregates.OrganizationDivision;
using RegOS.Persistence;

using DivisionAggregate = RegOS.Organization.Domain.Aggregates.OrganizationDivision.OrganizationDivision;

namespace RegOS.Organization.Infrastructure.Persistence;

public sealed class OrganizationDivisionRepository
    : IOrganizationDivisionRepository
{
    private readonly RegOSDbContext _dbContext;

    public OrganizationDivisionRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        DivisionAggregate division,
        CancellationToken cancellationToken)
    {
        await _dbContext.OrganizationDivisions.AddAsync(division, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<DivisionAggregate?> GetByIdAsync(
        OrganizationDivisionId id,
        CancellationToken cancellationToken)
        => await _dbContext.OrganizationDivisions
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task UpdateAsync(
        DivisionAggregate division,
        CancellationToken cancellationToken)
    {
        _dbContext.OrganizationDivisions.Update(division);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
