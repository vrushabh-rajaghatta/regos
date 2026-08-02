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
        var existing = await _dbContext.ApplicationTypes
            .ToListAsync(cancellationToken);

        var byId = existing.ToDictionary(x => x.Id);

        var missing = ApplicationTypes.Data
            .Where(x => !byId.ContainsKey(x.Id))
            .ToList();

        if (missing.Count > 0)
            _dbContext.ApplicationTypes.AddRange(missing);

        // Insert-only is not enough for the wire token (EPIC-007a S003). These
        // rows were seeded before the column existed, so on an already-upgraded
        // database every one of them is present and every one of them would keep
        // a null forever — while a fresh clone got `fdaat4` from the seed. The
        // two must converge, and reconciling here rather than in the migration
        // keeps one source of truth for the value.
        foreach (var seed in ApplicationTypes.Data)
        {
            if (byId.TryGetValue(seed.Id, out var row) && row.Token != seed.Token)
                row.RecordToken(seed.Token);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
