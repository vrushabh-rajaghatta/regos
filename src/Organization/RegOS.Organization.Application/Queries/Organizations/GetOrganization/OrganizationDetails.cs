using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Organization.Application.Queries.Organizations.GetOrganization;

/// <summary>
/// Everything the workspace overview shows about who this company is.
/// </summary>
/// <remarks>
/// The identity attributes below — acronym, native-language name, status date
/// and identifiers — reached the aggregate in EPIC-016 S003 and had no reader
/// until S004. A field that is modelled and persisted but absent from every
/// projection is not a capability; it is weight.
/// </remarks>
public sealed record OrganizationDetails(
    Guid Id,
    string LegalName,
    OrganizationType Type,
    OrganizationStatus Status,
    DateOnly StatusDate,
    string? Acronym,
    string? NameNativeLanguage,
    IReadOnlyList<OrganizationIdentifierDto> Identifiers);

/// <summary>
/// One registry identifier, carrying the scheme's code so the reader sees
/// "DUNS 150483782" rather than a pair of guids.
/// </summary>
public sealed record OrganizationIdentifierDto(
    Guid Id,
    Guid SchemeId,
    string SchemeCode,
    string SchemeName,
    string Value);
