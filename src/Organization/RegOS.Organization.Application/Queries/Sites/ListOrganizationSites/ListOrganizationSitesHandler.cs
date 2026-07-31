using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Application.Queries.Sites.SiteDirectory;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Persistence;

namespace RegOS.Organization.Application.Queries.Sites.ListOrganizationSites;

/// <summary>
/// "Which sites does this organization operate?" — the mirror of the directory,
/// scoped to one company.
/// </summary>
/// <remarks>
/// A separate handler rather than the directory with an organization filter, on
/// the read-model philosophy this platform has settled into: a read model is
/// shaped by the question, and these are two questions. This one implies the
/// organization; the directory has to name it in every row.
/// </remarks>
public sealed class ListOrganizationSitesHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListOrganizationSitesHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns the organization's sites, or null when the organization does not
    /// exist — so the endpoint can 404 rather than return an empty list for a
    /// company that was never there.
    /// </summary>
    public async Task<IReadOnlyList<OrganizationSiteRow>?> HandleAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken)
    {
        var organizationExists = await _dbContext.Organizations
            .AsNoTracking()
            .AnyAsync(x => x.Id == organizationId, cancellationToken);

        if (!organizationExists)
            return null;

        var sites = await (
            from site in _dbContext.OrganizationSites
                .AsNoTracking()
                .Include(x => x.Identifiers)
            where site.OrganizationId == organizationId
            join country in _dbContext.Countries
                on site.Address.CountryId equals country.Id
            orderby site.Name
            select new { site, CountryName = country.Name })
            .ToListAsync(cancellationToken);

        var schemes = await _dbContext.IdentifierSchemes
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);

        return sites
            .Select(row => new OrganizationSiteRow(
                row.site.Id.Value,
                row.site.Name,
                row.site.Type.ToString(),
                row.site.Address.CountryId.Value,
                row.CountryName,
                row.site.Address.City,
                row.site.Status.ToString(),
                row.site.StatusDate,
                [.. row.site.Identifiers
                    .OrderBy(x => schemes.GetValueOrDefault(x.SchemeId, string.Empty))
                    .Select(x => new SiteIdentifierDto(
                        x.Id.Value,
                        x.SchemeId.Value,
                        schemes.GetValueOrDefault(x.SchemeId, string.Empty),
                        x.Value))]))
            .ToList();
    }
}

/// <summary>A site as seen from inside its own organization.</summary>
public sealed record OrganizationSiteRow(
    Guid SiteId,
    string Name,
    string Type,
    Guid CountryId,
    string CountryName,
    string? City,
    string Status,
    DateOnly StatusDate,
    IReadOnlyList<SiteIdentifierDto> Identifiers);
