using Microsoft.EntityFrameworkCore;

namespace RegOS.Persistence.Initialization.ReferenceData;

public sealed class SubmissionTypeDataInitializer : IDataInitializer
{
    private readonly RegOSDbContext _dbContext;

    public SubmissionTypeDataInitializer(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        // Additive + idempotent, matched on deterministic id. Insert-only is
        // sufficient here — unlike ApplicationTypes, no row of this table has
        // ever existed without its token, so there is nothing to reconcile.
        // SubmissionTypes are global (no tenant query filter).
        var existingIds = await _dbContext.SubmissionTypes
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var missing = SubmissionTypes.Data
            .Where(x => !existingIds.Contains(x.Id))
            .ToList();

        if (missing.Count > 0)
        {
            _dbContext.SubmissionTypes.AddRange(missing);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
