using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Application.Queries.Sites.SiteDirectory;
using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.Persistence;

namespace RegOS.Organization.Application.Queries.Sites.GetOrganizationSite;

public sealed class GetOrganizationSiteHandler
{
    private readonly RegOSDbContext _dbContext;

    public GetOrganizationSiteHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns the site with the names a person reads, or null when it does not
    /// exist — so the endpoint can 404 rather than return an empty one.
    /// </summary>
    public async Task<OrganizationSiteDetails?> HandleAsync(
        GetOrganizationSiteQuery query,
        CancellationToken cancellationToken)
    {
        var id = query.SiteId;

        var site = await _dbContext.OrganizationSites
            .AsNoTracking()
            .Include(x => x.Identifiers)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (site is null)
            return null;

        var organizationName = await _dbContext.Organizations
            .AsNoTracking()
            .Where(x => x.Id == site.OrganizationId)
            .Select(x => x.LegalName)
            .FirstOrDefaultAsync(cancellationToken);

        var countryName = await _dbContext.Countries
            .AsNoTracking()
            .Where(x => x.Id == site.Address.CountryId)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken);

        var schemeIds = site.Identifiers.Select(x => x.SchemeId).ToList();

        var schemes = await _dbContext.IdentifierSchemes
            .AsNoTracking()
            .Where(x => schemeIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);

        return new OrganizationSiteDetails(
            site.Id.Value,
            site.OrganizationId.Value,
            organizationName ?? string.Empty,
            site.Name,
            site.NameNativeLanguage,
            site.Type.ToString(),
            site.Status.ToString(),
            site.StatusDate,
            site.Email,
            site.Phone,
            new SiteAddressDto(
                site.Address.CountryId.Value,
                countryName ?? string.Empty,
                site.Address.Line1,
                site.Address.Line2,
                site.Address.Line3,
                site.Address.City,
                site.Address.StateProvince,
                site.Address.PostalCode),
            [.. site.Identifiers
                // Deterministic: a site carries one identifier per scheme —
                // the unique index on (OrganizationSiteId, SchemeId).
                .OrderBy(x => schemes.GetValueOrDefault(x.SchemeId, string.Empty))
                .Select(x => new SiteIdentifierDto(
                    x.Id.Value,
                    x.SchemeId.Value,
                    schemes.GetValueOrDefault(x.SchemeId, string.Empty),
                    x.Value))]);
    }
}
