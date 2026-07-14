using Microsoft.EntityFrameworkCore;

using RegOS.MasterData.Domain.Regulatory.Authority;
using RegOS.Persistence;

namespace RegOS.ReferenceData.Application.Queries.SubmissionTypes.ListSubmissionTypes;

public sealed class ListSubmissionTypesHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListSubmissionTypesHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SubmissionTypeDto>> HandleAsync(
        Guid? authorityId,
        CancellationToken cancellationToken = default)
    {
        // Read model: only active reference records, ordered by display name
        // so lookups/dropdowns are deterministic. IsActive is not exposed.
        // One composable query, optionally narrowed to a single Authority.
        var query = _dbContext.SubmissionTypes
            .AsNoTracking()
            .Where(x => x.IsActive);

        if (authorityId.HasValue)
        {
            var id = new AuthorityId(authorityId.Value);
            query = query.Where(x => x.AuthorityId == id);
        }

        return await query
            .OrderBy(x => x.Name)
            .Select(x => new SubmissionTypeDto(
                x.Id,
                x.Code,
                x.Name,
                x.AuthorityId))
            .ToListAsync(cancellationToken);
    }
}
