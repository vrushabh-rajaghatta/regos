using Microsoft.EntityFrameworkCore;

using RegOS.Labeling.Domain.Aggregates.UndesirableEffects;
using RegOS.Persistence;

namespace RegOS.Labeling.Infrastructure.Repositories;

public sealed class UndesirableEffectRepository : IUndesirableEffectRepository
{
    private readonly RegOSDbContext _dbContext;

    public UndesirableEffectRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(UndesirableEffect statement, CancellationToken cancellationToken)
    {
        await _dbContext.UndesirableEffects.AddAsync(statement, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Tracked, with populations.
    /// </summary>
    /// <remarks>
    /// No <c>Include</c>: populations are an owned collection, and EF loads an
    /// owner's owned types with it. That is a consequence of the mapping S004
    /// chose, not an omission — and it is why the rules that read the collection
    /// cannot silently see an empty one.
    /// </remarks>
    public async Task<UndesirableEffect?> GetByIdAsync(
        UndesirableEffectId id,
        CancellationToken cancellationToken)
        => await _dbContext.UndesirableEffects
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task UpdateAsync(
        UndesirableEffect statement,
        CancellationToken cancellationToken)
        => await _dbContext.SaveChangesAsync(cancellationToken);
}
