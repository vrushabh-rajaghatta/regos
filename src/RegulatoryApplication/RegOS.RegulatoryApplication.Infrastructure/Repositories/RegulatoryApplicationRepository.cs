using Microsoft.EntityFrameworkCore;

using RegOS.Product.Domain.Product;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.RegulatoryApplication.Infrastructure.Persistence;
using RegulatoryApplicationAggregate = RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;

namespace RegOS.RegulatoryApplication.Infrastructure.Repositories;

public sealed class RegulatoryApplicationRepository
    : IRegulatoryApplicationRepository
{
    private readonly RegulatoryApplicationDbContext _dbContext;

    public RegulatoryApplicationRepository(
        RegulatoryApplicationDbContext dbContext)
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
