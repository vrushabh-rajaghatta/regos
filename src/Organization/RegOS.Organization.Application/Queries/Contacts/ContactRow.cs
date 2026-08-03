namespace RegOS.Organization.Application.Queries.Contacts;

/// <summary>
/// A person as the directory shows them — the answer to <em>"who is the QP for
/// this application?"</em>, which spans the registry rather than one company.
/// </summary>
public sealed record ContactRow(
    Guid ContactId,
    string FirstName,
    string LastName,
    string? Title,
    string? Department,
    Guid OrganizationId,
    string OrganizationName,
    Guid? SiteId,
    string? SiteName,
    string Status,
    DateOnly StatusDate,
    IReadOnlyList<ContactRoleDto> Roles,
    IReadOnlyList<string> Emails,
    IReadOnlyList<ContactPhoneDto> Phones);

public sealed record ContactRoleDto(Guid RoleId, string Code, string Name);

/// <param name="Kind">
/// <c>Business</c>, <c>Fax</c>, <c>Mobile</c> — or <b>null, meaning the number
/// was recorded before RegOS asked</b>. Sent as the name rather than an
/// ordinal, so a client reading the payload sees what it means.
/// </param>
public sealed record ContactPhoneDto(string Number, string? Kind);
