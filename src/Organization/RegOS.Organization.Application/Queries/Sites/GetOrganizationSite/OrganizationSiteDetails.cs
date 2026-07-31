using RegOS.Organization.Application.Queries.Sites.SiteDirectory;

namespace RegOS.Organization.Application.Queries.Sites.GetOrganizationSite;

public sealed record OrganizationSiteDetails(
    Guid Id,
    Guid OrganizationId,
    string OrganizationName,
    string Name,
    string? NameNativeLanguage,
    string Type,
    string Status,
    DateOnly StatusDate,
    string? Email,
    string? Phone,
    SiteAddressDto Address,
    IReadOnlyList<SiteIdentifierDto> Identifiers);

/// <param name="CountryName">
/// The only part of an address the model reasons about — everything else is
/// descriptive and may legitimately be absent.
/// </param>
public sealed record SiteAddressDto(
    Guid CountryId,
    string CountryName,
    string? Line1,
    string? Line2,
    string? Line3,
    string? City,
    string? StateProvince,
    string? PostalCode);
