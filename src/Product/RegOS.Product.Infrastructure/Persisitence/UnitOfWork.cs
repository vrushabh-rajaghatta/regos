namespace RegOS.Product.Infrastructure.Persistence;

using RegOS.Persistence;
using RegOS.Product.Application.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly RegOSDbContext _dbContext;

    public UnitOfWork(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}