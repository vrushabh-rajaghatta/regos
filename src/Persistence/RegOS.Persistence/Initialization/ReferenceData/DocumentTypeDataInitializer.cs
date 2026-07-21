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
        // every time and this would re-insert on every boot (ADR-031).
        if (!await _dbContext.DocumentTypes
                .IgnoreQueryFilters()
                .AnyAsync(cancellationToken))
        {
            _dbContext.DocumentTypes.AddRange(DocumentTypes.Data);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
