using Microsoft.EntityFrameworkCore;

namespace RegOS.Persistence.Initialization.ReferenceData;

public sealed class AuthorityDivisionDataInitializer : IDataInitializer
{
    private readonly RegOSDbContext _dbContext;

    public AuthorityDivisionDataInitializer(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        // IgnoreQueryFilters: seeding runs with no tenant, and the
        // platform-seeded rows would otherwise be invisible to the very check
        // that decides whether to insert them again.
        var existingIds = await _dbContext.AuthorityDivisions
            .IgnoreQueryFilters()
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var missing = AuthorityDivisions.Data
            .Where(x => !existingIds.Contains(x.Id))
            .ToList();

        if (missing.Count > 0)
        {
            _dbContext.AuthorityDivisions.AddRange(missing);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
