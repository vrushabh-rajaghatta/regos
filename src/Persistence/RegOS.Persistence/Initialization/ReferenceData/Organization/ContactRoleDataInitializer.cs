using Microsoft.EntityFrameworkCore;

namespace RegOS.Persistence.Initialization.ReferenceData.Organization;

public sealed class ContactRoleDataInitializer : IDataInitializer
{
    private readonly RegOSDbContext _dbContext;

    public ContactRoleDataInitializer(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        // Additive + idempotent, and IgnoreQueryFilters because seeding runs
        // with no tenant: the shared-plus-extensible filter would otherwise hide
        // every existing row and the seeder would try to insert them all again.
        var existingIds = await _dbContext.ContactRoles
            .IgnoreQueryFilters()
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var missing = ContactRoles.Data
            .Where(x => !existingIds.Contains(x.Id))
            .ToList();

        if (missing.Count > 0)
        {
            _dbContext.ContactRoles.AddRange(missing);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
