using Microsoft.EntityFrameworkCore;

using RegOS.Labeling.Domain.Aggregates.GlobalLabels;
using RegOS.Persistence;

namespace RegOS.Labeling.Infrastructure.Repositories;

public sealed class GlobalLabelRepository : IGlobalLabelRepository
{
    private readonly RegOSDbContext _dbContext;

    public GlobalLabelRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        GlobalLabel globalLabel,
        CancellationToken cancellationToken)
    {
        await _dbContext.GlobalLabels.AddAsync(globalLabel, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Tracked, with versions — always, never on request.
    /// </summary>
    /// <remarks>
    /// Every rule on this aggregate is a statement about the <em>set</em> of
    /// versions: at most one draft, at most one in force, the next number is one
    /// past the highest. A label loaded without them would enforce all three
    /// against an empty collection and quietly succeed.
    /// </remarks>
    public async Task<GlobalLabel?> GetByIdAsync(
        GlobalLabelId id,
        CancellationToken cancellationToken)
        => await _dbContext.GlobalLabels
            .Include(x => x.Versions)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task UpdateAsync(
        GlobalLabel globalLabel,
        CancellationToken cancellationToken)
        => await _dbContext.SaveChangesAsync(cancellationToken);
}
