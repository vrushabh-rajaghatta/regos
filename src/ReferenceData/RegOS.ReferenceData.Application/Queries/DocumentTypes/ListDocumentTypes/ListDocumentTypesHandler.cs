using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.ReferenceData.Application.Queries.DocumentTypes.ListDocumentTypes;

public sealed class ListDocumentTypesHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListDocumentTypesHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<DocumentTypeDto>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        // Only active types, ordered by display name. System types only for
        // now; organization extensions and org filtering arrive later.
        return await _dbContext.DocumentTypes
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new DocumentTypeDto(
                x.Id,
                x.Code,
                x.Name,
                x.Description))
            .ToListAsync(cancellationToken);
    }
}
