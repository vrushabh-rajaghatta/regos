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
        if (!await _dbContext.DocumentTypes.AnyAsync(cancellationToken))
        {
            _dbContext.DocumentTypes.AddRange(DocumentTypes.Data);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
