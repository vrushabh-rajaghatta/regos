using Microsoft.EntityFrameworkCore;

namespace RegOS.Persistence.Initialization.ReferenceData.Blueprint;

public sealed class RegulatoryTemplateDataInitializer : IDataInitializer
{
    private readonly RegOSDbContext _dbContext;

    public RegulatoryTemplateDataInitializer(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        // IgnoreQueryFilters: startup has no tenant, and templates carry the
        // shared-plus-tenant filter (ADR-031) — without this the filter would
        // report an empty table and re-insert on every boot.
        //
        // Additive + idempotent: insert only the templates whose deterministic
        // ids are missing.
        var existingIds = await _dbContext.RegulatoryTemplates
            .IgnoreQueryFilters()
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var missing = RegulatoryTemplates.Data
            .Where(x => !existingIds.Contains(x.Id))
            .ToList();

        if (missing.Count > 0)
        {
            _dbContext.RegulatoryTemplates.AddRange(missing);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
