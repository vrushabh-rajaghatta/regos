using Microsoft.EntityFrameworkCore;

namespace RegOS.Persistence.Initialization.ReferenceData.Organization;

public sealed class IdentifierSchemeDataInitializer : IDataInitializer
{
    private readonly RegOSDbContext _dbContext;

    public IdentifierSchemeDataInitializer(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        // Additive + idempotent: insert only the seed rows whose deterministic
        // ids are not already present, so a newly added scheme lands on an
        // existing database without wiping the table. Schemes are global (no
        // tenant query filter).
        var existingIds = await _dbContext.IdentifierSchemes
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var missing = IdentifierSchemes.Data
            .Where(x => !existingIds.Contains(x.Id))
            .ToList();

        if (missing.Count > 0)
        {
            _dbContext.IdentifierSchemes.AddRange(missing);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
