using Microsoft.EntityFrameworkCore;

namespace RegOS.Persistence.Initialization.ReferenceData;

public sealed class CorrespondenceTypeDataInitializer : IDataInitializer
{
    private readonly RegOSDbContext _dbContext;

    public CorrespondenceTypeDataInitializer(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        // Additive + idempotent, like every other reference seed: insert only
        // the deterministic ids not already present, so a ninth type lands on
        // an existing database without wiping the table.
        var existingIds = await _dbContext.CorrespondenceTypes
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var missing = CorrespondenceTypes.Data
            .Where(x => !existingIds.Contains(x.Id))
            .ToList();

        if (missing.Count > 0)
        {
            _dbContext.CorrespondenceTypes.AddRange(missing);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
