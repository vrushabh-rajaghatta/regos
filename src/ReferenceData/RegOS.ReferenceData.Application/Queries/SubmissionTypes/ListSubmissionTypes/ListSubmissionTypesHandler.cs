using Microsoft.EntityFrameworkCore;

using RegOS.ReferenceData.Domain.Regulatory.Authority;
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
        ListSubmissionTypesQuery query,
        CancellationToken cancellationToken = default)
    {
        var authorityId = query.AuthorityId;

        // Read model: only active reference records, ordered by display name so
        // dropdowns are deterministic. IsActive is not exposed.
        var types = _dbContext.SubmissionTypes
            .AsNoTracking()
            .Where(x => x.IsActive);

        if (authorityId.HasValue)
        {
            var id = new AuthorityId(authorityId.Value);
            types = types.Where(x => x.AuthorityId == id);
        }

        return await types
            .OrderBy(x => x.Name)
            .Select(x => new SubmissionTypeDto(
                x.Id,
                x.Code,
                x.Name,
                x.Token,
                x.AuthorityId))
            .ToListAsync(cancellationToken);
    }
}
