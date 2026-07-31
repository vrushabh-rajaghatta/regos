using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.Persistence;
using RegOS.ReferenceData.Domain.Geography.Country;

namespace RegOS.Organization.Application.Queries.Sites.SiteDirectory;

/// <summary>
/// "Which sites do we have, where, and of what kind?" — across the tenant's
/// whole registry rather than within one organization.
/// </summary>
/// <remarks>
/// This query is the argument for <c>OrganizationSite</c> being an aggregate
/// root, so it ships with the aggregate rather than a story later: a root
/// justified by a query that does not exist yet is a demo of an empty table.
/// <para>
/// Both filters are optional and neither is a default. Nothing is hidden —
/// inactive sites are returned and marked, because a site that closed last year
/// is still the site named on a licence granted in 2019.
/// </para>
/// </remarks>
public sealed class SiteDirectoryHandler
{
    private readonly RegOSDbContext _dbContext;

    public SiteDirectoryHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SiteDirectoryRow>> HandleAsync(
        SiteDirectoryQuery query,
        CancellationToken cancellationToken)
    {
        var (countryId, type) = (query.CountryId, query.Type);

        var sites = _dbContext.OrganizationSites.AsNoTracking();

        if (countryId is { } country)
            sites = sites.Where(x => x.Address.CountryId == country);

        if (type is { } siteType)
            sites = sites.Where(x => x.Type == siteType);

        // Strongly-typed ids are materialised then unwrapped in memory: their
        // converters have no SQL translation for .Value.
        var rows = await (
            from site in sites.Include(x => x.Identifiers)
            join organization in _dbContext.Organizations
                on site.OrganizationId equals organization.Id
            join countryRow in _dbContext.Countries
                on site.Address.CountryId equals countryRow.Id
            orderby countryRow.Name, organization.LegalName, site.Name
            select new
            {
                site,
                OrganizationName = organization.LegalName,
                CountryName = countryRow.Name,
            }).ToListAsync(cancellationToken);

        var schemes = await _dbContext.IdentifierSchemes
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);

        return rows
            .Select(row => new SiteDirectoryRow(
                row.site.Id.Value,
                row.site.Name,
                row.site.Type.ToString(),
                row.site.OrganizationId.Value,
                row.OrganizationName,
                row.site.Address.CountryId.Value,
                row.CountryName,
                row.site.Address.City,
                row.site.Status.ToString(),
                row.site.StatusDate,
                [.. row.site.Identifiers
                    .OrderBy(identifier => schemes.GetValueOrDefault(
                        identifier.SchemeId, string.Empty))
                    .Select(identifier => new SiteIdentifierDto(
                        identifier.Id.Value,
                        identifier.SchemeId.Value,
                        schemes.GetValueOrDefault(
                            identifier.SchemeId, string.Empty),
                        identifier.Value))]))
            .ToList();
    }
}
