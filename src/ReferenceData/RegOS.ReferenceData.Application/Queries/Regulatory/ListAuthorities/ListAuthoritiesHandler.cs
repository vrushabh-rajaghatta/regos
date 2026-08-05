using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.ReferenceData.Application.Queries.Regulatory.ListAuthorities;

public sealed class ListAuthoritiesHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListAuthoritiesHandler(
        RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AuthorityDto>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Authorities
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .ThenBy(a => a.Id)
            .Select(a => new AuthorityDto(
                a.Id,
                a.Code,
                a.Name,
                a.CountryId))
            .ToListAsync(cancellationToken);
    }
}
