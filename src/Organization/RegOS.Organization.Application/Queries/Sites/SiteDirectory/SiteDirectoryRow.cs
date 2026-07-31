namespace RegOS.Organization.Application.Queries.Sites.SiteDirectory;

/// <summary>
/// A row in the tenant-wide site directory — the answer to <em>"which
/// manufacturing sites do we have in India?"</em>.
/// </summary>
/// <remarks>
/// Carries the organization, because the directory spans the whole registry and
/// the owning company is not implied by where you are standing. That is the
/// query that made <c>OrganizationSite</c> an aggregate root rather than a child
/// of <c>Organization</c>.
/// </remarks>
public sealed record SiteDirectoryRow(
    Guid SiteId,
    string Name,
    string Type,
    Guid OrganizationId,
    string OrganizationName,
    Guid CountryId,
    string CountryName,
    string? City,
    string Status,
    DateOnly StatusDate,
    IReadOnlyList<SiteIdentifierDto> Identifiers);

public sealed record SiteIdentifierDto(
    Guid Id,
    Guid SchemeId,
    string SchemeCode,
    string Value);
