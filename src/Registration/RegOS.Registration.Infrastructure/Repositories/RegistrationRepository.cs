using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.Registration.Domain.Aggregates.Registration;

using RegistrationAggregate = RegOS.Registration.Domain.Aggregates.Registration.Registration;

namespace RegOS.Registration.Infrastructure.Repositories;

public sealed class RegistrationRepository : IRegistrationRepository
{
    private readonly RegOSDbContext _dbContext;

    public RegistrationRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        RegistrationAggregate registration,
        CancellationToken cancellationToken)
    {
        _dbContext.Registrations.Add(registration);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Tracked, with history — the aggregate appends to it, so it has to be
    /// loaded or an added entry would be silently dropped.
    /// </summary>
    public async Task<RegistrationAggregate?> GetByIdAsync(
        RegistrationId id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Registrations
            .Include(x => x.History)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(
        RegistrationAggregate registration,
        CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RegistrationAggregate>> ListByProductAsync(
        GlobalProductId globalProductId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Registrations
            .Where(x => x.GlobalProductId == globalProductId)
            .OrderBy(x => x.CreatedOn)
            .ToListAsync(cancellationToken);
    }
}
