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
        return await _dbContext.RegulatoryApplications
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<RegulatoryApplicationAggregate>> ListByProductAsync(
        ProductId productId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.RegulatoryApplications
            .Where(x => x.ProductId == productId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}
