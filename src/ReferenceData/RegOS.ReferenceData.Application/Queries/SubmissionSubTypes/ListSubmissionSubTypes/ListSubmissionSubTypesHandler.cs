using Microsoft.EntityFrameworkCore;

using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Persistence;

namespace RegOS.ReferenceData.Application.Queries.SubmissionSubTypes.ListSubmissionSubTypes;

public sealed class ListSubmissionSubTypesHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListSubmissionSubTypesHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SubmissionSubTypeDto>> HandleAsync(
        ListSubmissionSubTypesQuery query,
        CancellationToken cancellationToken = default)
    {
        var authorityId = query.AuthorityId;

        // Read model: only active reference records, ordered by display name so
        // dropdowns are deterministic. IsActive is not exposed.
        var types = _dbContext.SubmissionSubTypes
            .AsNoTracking()
            .Where(x => x.IsActive);

        if (authorityId.HasValue)
        {
            var id = new AuthorityId(authorityId.Value);
            types = types.Where(x => x.AuthorityId == id);
        }

        return await types
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Select(x => new SubmissionSubTypeDto(
                x.Id,
                x.Code,
                x.Name,
                x.Token,
                x.AuthorityId))
            .ToListAsync(cancellationToken);
    }
}
