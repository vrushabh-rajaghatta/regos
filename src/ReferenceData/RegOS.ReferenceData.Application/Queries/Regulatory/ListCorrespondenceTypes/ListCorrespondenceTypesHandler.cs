using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.ReferenceData.Application.Queries.Regulatory.ListCorrespondenceTypes;

public sealed class ListCorrespondenceTypesHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListCorrespondenceTypesHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CorrespondenceTypeDto>> HandleAsync(
        ListCorrespondenceTypesQuery query,
        CancellationToken cancellationToken = default)
        => await _dbContext.CorrespondenceTypes
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new CorrespondenceTypeDto(
                x.Id.Value,
                x.Code,
                x.Name))
            .ToListAsync(cancellationToken);
}
