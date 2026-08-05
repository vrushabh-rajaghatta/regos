using Microsoft.EntityFrameworkCore;

using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Persistence;

namespace RegOS.ReferenceData.Application.Queries.ApplicationTypes.ListApplicationTypes;

public sealed class ListApplicationTypesHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListApplicationTypesHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ApplicationTypeDto>> HandleAsync(
        ListApplicationTypesQuery query,
        CancellationToken cancellationToken = default)
    {
        var authorityId = query.AuthorityId;

        // Read model: only active reference records, ordered by display name
        // so lookups/dropdowns are deterministic. IsActive is not exposed.
        // One composable query, optionally narrowed to a single Authority.
        var types = _dbContext.ApplicationTypes
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
            .Select(x => new ApplicationTypeDto(
                x.Id,
                x.Code,
                x.Name,
                x.AuthorityId))
            .ToListAsync(cancellationToken);
    }
}
