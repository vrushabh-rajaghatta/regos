namespace RegOS.Api.Endpoints.Contacts;

/// <param name="OrganizationSiteId">
/// Optional — a head-office regulatory lead or an authority reviewer has no
/// site.
/// </param>
/// <param name="StatusDate">The business date this person took up the post.</param>
public sealed record CreateContactRequest(
    string FirstName,
    string LastName,
    DateOnly StatusDate,
    Guid? OrganizationSiteId = null,
    string? Title = null,
    string? Department = null,
    Guid? CountryId = null,
    IReadOnlyList<Guid>? RoleIds = null,
    IReadOnlyList<string>? Emails = null,
    IReadOnlyList<string>? Phones = null);

public sealed record CreateContactResponse(Guid Id);
