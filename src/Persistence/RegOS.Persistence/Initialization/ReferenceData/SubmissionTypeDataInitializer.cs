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
        // Additive + idempotent: insert only the seed rows whose deterministic
        // ids are not already present, so newly added reference data lands on
        // an existing database without wiping the table. SubmissionTypes are
        // global (no tenant query filter).
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
