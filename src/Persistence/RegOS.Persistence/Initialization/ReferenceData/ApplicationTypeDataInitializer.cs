using Microsoft.EntityFrameworkCore;

namespace RegOS.Persistence.Initialization.ReferenceData;

public sealed class ApplicationTypeDataInitializer : IDataInitializer
{
    private readonly RegOSDbContext _dbContext;

    public ApplicationTypeDataInitializer(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        // Additive + idempotent: insert only the seed rows whose deterministic
        // ids are not already present, so newly added reference data lands on
        // an existing database without wiping the table. ApplicationTypes are
        // global (no tenant query filter).
        var existingIds = await _dbContext.ApplicationTypes
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var missing = ApplicationTypes.Data
            .Where(x => !existingIds.Contains(x.Id))
            .ToList();

        if (missing.Count > 0)
        {
            _dbContext.ApplicationTypes.AddRange(missing);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
