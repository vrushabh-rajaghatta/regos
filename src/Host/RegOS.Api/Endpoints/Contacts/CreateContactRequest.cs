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
    IReadOnlyList<CreateContactPhone>? Phones = null);

/// <param name="Kind">
/// <c>Business</c>, <c>Fax</c> or <c>Mobile</c>, by name. <b>Null is a legal
/// answer</b> and means the caller does not know — it is not the API declining
/// to validate, it is the domain declining to guess.
/// </param>
public sealed record CreateContactPhone(string Number, string? Kind);

public sealed record CreateContactResponse(Guid Id);
