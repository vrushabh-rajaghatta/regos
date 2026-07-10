namespace RegOS.Product.Infrastructure.Persistence;

using RegOS.Product.Application.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ProductDbContext _dbContext;

    public UnitOfWork(ProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}