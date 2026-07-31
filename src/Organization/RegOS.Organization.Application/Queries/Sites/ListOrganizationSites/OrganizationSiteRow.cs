using RegOS.Organization.Application.Queries.Sites.SiteDirectory;

namespace RegOS.Organization.Application.Queries.Sites.ListOrganizationSites;

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
