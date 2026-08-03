using Microsoft.EntityFrameworkCore;

namespace RegOS.Persistence.Initialization.ReferenceData;

public sealed class SubstanceDataInitializer : IDataInitializer
{
    private readonly RegOSDbContext _dbContext;

    public SubstanceDataInitializer(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        // IgnoreQueryFilters: seeding runs with no tenant, and the shared rows
        // would otherwise be invisible to the very check that decides whether
        // to insert them again.
        var existingIds = await _dbContext.Substances
            .IgnoreQueryFilters()
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var missing = Substances.Data
            .Where(x => !existingIds.Contains(x.Id))
            .ToList();

        if (missing.Count > 0)
        {
            _dbContext.Substances.AddRange(missing);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
