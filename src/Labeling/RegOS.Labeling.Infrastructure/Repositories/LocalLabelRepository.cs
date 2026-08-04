using Microsoft.EntityFrameworkCore;

using RegOS.Labeling.Domain.Aggregates.LocalLabels;
using RegOS.Persistence;

namespace RegOS.Labeling.Infrastructure.Repositories;

public sealed class LocalLabelRepository : ILocalLabelRepository
{
    private readonly RegOSDbContext _dbContext;

    public LocalLabelRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        LocalLabel localLabel,
        CancellationToken cancellationToken)
    {
        await _dbContext.LocalLabels.AddAsync(localLabel, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Tracked, with revisions — always, never on request.
    /// </summary>
    /// <remarks>
    /// Every rule on this aggregate is a statement about the <em>set</em> of
    /// revisions: at most one draft, at most one in force, the next number is
    /// one past the highest. A label loaded without them would enforce all three
    /// against an empty collection and quietly succeed.
    /// </remarks>
    public async Task<LocalLabel?> GetByIdAsync(
        LocalLabelId id,
        CancellationToken cancellationToken)
        => await _dbContext.LocalLabels
            .Include(x => x.Revisions)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task UpdateAsync(
        LocalLabel localLabel,
        CancellationToken cancellationToken)
        => await _dbContext.SaveChangesAsync(cancellationToken);
}
