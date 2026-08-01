using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.ReferenceData.Application.Queries.Regulatory.ListAuthorityDivisions;

public sealed class ListAuthorityDivisionsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListAuthorityDivisionsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AuthorityDivisionDto>> HandleAsync(
        ListAuthorityDivisionsQuery query,
        CancellationToken cancellationToken = default)
        // The platform's and this tenant's, never another's — the global
        // query filter does that, not this handler (ADR-031).
        => await _dbContext.AuthorityDivisions
            .AsNoTracking()
            .Where(x => x.AuthorityId == query.AuthorityId && x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new AuthorityDivisionDto(
                x.Id.Value,
                x.AuthorityId.Value,
                x.Name,
                x.TenantId != null))
            .ToListAsync(cancellationToken);
}
