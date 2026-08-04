using Microsoft.EntityFrameworkCore;

using RegOS.Labeling.Domain.Aggregates.Indications;
using RegOS.Persistence;

namespace RegOS.Labeling.Infrastructure.Repositories;

public sealed class IndicationRepository : IIndicationRepository
{
    private readonly RegOSDbContext _dbContext;

    public IndicationRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Indication indication,
        CancellationToken cancellationToken)
    {
        await _dbContext.Indications.AddAsync(indication, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Tracked, with all three collections — always.
    /// </summary>
    /// <remarks>
    /// The status rules read the history ("not the status already in force",
    /// "business time moves forward"), and the population rules read the
    /// collection. A load that omitted either would enforce those rules against
    /// an empty list and quietly succeed — the EPIC-004 S005 lesson.
    /// </remarks>
    public async Task<Indication?> GetByIdAsync(
        IndicationId id,
        CancellationToken cancellationToken)
        => await _dbContext.Indications
            .Include(x => x.Populations)
            .Include(x => x.OtherTherapies)
            .Include(x => x.StatusHistory)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task UpdateAsync(
        Indication indication,
        CancellationToken cancellationToken)
        => await _dbContext.SaveChangesAsync(cancellationToken);
}
