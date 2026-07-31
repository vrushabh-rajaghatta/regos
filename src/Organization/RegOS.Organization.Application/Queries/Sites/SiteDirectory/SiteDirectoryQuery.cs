using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.ReferenceData.Domain.Geography.Country;

namespace RegOS.Organization.Application.Queries.Sites.SiteDirectory;

/// <summary>
/// "Which sites do we have, where, and of what kind?" — across the tenant's
/// whole registry rather than within one organization.
/// </summary>
/// <param name="CountryId">Optional; no default.</param>
/// <param name="Type">Optional; no default.</param>
public sealed record SiteDirectoryQuery(
    CountryId? CountryId = null,
    OrganizationSiteType? Type = null);
