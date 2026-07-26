using Microsoft.EntityFrameworkCore;

namespace RegOS.Persistence.Initialization.ReferenceData;

public sealed class DocumentTypeDataInitializer : IDataInitializer
{
    private readonly RegOSDbContext _dbContext;

    public DocumentTypeDataInitializer(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        // IgnoreQueryFilters: initializers run at startup with no request
        // and therefore no tenant; the filter would report an empty table
        // every time (ADR-031).
        //
        // Additive + idempotent: insert only the seed rows whose deterministic
        // ids are not already present, so newly added reference data lands on
        // an existing database without wiping the table.
        var existingIds = await _dbContext.DocumentTypes
            .IgnoreQueryFilters()
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var missing = DocumentTypes.Data
            .Where(x => !existingIds.Contains(x.Id))
            .ToList();

        if (missing.Count > 0)
        {
            _dbContext.DocumentTypes.AddRange(missing);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
