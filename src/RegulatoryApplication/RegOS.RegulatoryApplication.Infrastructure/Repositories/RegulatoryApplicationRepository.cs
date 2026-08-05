using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegulatoryApplicationAggregate = RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;

namespace RegOS.RegulatoryApplication.Infrastructure.Repositories;

public sealed class RegulatoryApplicationRepository
    : IRegulatoryApplicationRepository
{
    private readonly RegOSDbContext _dbContext;

    public RegulatoryApplicationRepository(
        RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        RegulatoryApplicationAggregate regulatoryApplication,
        CancellationToken cancellationToken)
    {
        _dbContext.RegulatoryApplications.Add(regulatoryApplication);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<RegulatoryApplicationAggregate?> GetByIdAsync(
        RegulatoryApplicationId id,
        CancellationToken cancellationToken)
    {
        // Tracked, with its citations. Load-bearing rather than convenient:
        // the aggregate's idempotence check reads the collection, so an
        // unloaded one reports "not cited yet" and adds a second row for a
        // study already there. The unique index caught exactly that.
        return await _dbContext.RegulatoryApplications
            .Include(x => x.StudyCitations)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RegulatoryApplicationAggregate>> ListByProductAsync(
        GlobalProductId globalProductId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.RegulatoryApplications
            .Where(x => x.GlobalProductId == globalProductId)
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }
}
