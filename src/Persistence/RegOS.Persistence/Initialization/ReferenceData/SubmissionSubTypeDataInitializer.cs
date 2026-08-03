using Microsoft.EntityFrameworkCore;

namespace RegOS.Persistence.Initialization.ReferenceData;

public sealed class SubmissionSubTypeDataInitializer : IDataInitializer
{
    private readonly RegOSDbContext _dbContext;

    public SubmissionSubTypeDataInitializer(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        // Additive + idempotent, matched on deterministic id. See
        // SubmissionTypeDataInitializer for why insert-only is enough.
        var existingIds = await _dbContext.SubmissionSubTypes
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var missing = SubmissionSubTypes.Data
            .Where(x => !existingIds.Contains(x.Id))
            .ToList();

        if (missing.Count > 0)
        {
            _dbContext.SubmissionSubTypes.AddRange(missing);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
