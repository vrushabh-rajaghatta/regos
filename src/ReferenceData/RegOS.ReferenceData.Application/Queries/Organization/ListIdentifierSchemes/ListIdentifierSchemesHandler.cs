using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.ReferenceData.Application.Queries.Organization.ListIdentifierSchemes;

/// <summary>
/// The registries that issue identifiers to companies and their sites.
/// </summary>
/// <remarks>
/// Unfiltered on purpose: schemes are world facts, not a tenant's own list, and
/// the DUNS registry is the same registry for everyone (the third filter shape
/// in <c>RegOSDbContext</c>'s remarks).
/// </remarks>
public sealed class ListIdentifierSchemesHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListIdentifierSchemesHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<IdentifierSchemeDto>> HandleAsync(
        ListIdentifierSchemesQuery query,
        CancellationToken cancellationToken)
        => await _dbContext.IdentifierSchemes
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .Select(x => new IdentifierSchemeDto(
                x.Id.Value,
                x.Code,
                x.Name,
                x.Issuer))
            .ToListAsync(cancellationToken);
}
