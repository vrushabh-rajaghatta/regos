using RegOS.Organization.Domain.Aggregates.Contact;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Organization;

namespace RegOS.Organization.Application.Commands.CreateContact;

/// <param name="StatusDate">
/// The business date this person took up the post — supplied rather than read
/// from the clock, like every other date in RegOS.
/// </param>
public sealed record CreateContactCommand(
    OrganizationId OrganizationId,
    string FirstName,
    string LastName,
    DateOnly StatusDate,
    OrganizationSiteId? OrganizationSiteId = null,
    string? Title = null,
    string? Department = null,
    CountryId? CountryId = null,
    IReadOnlyList<ContactRoleId>? RoleIds = null,
    IReadOnlyList<string>? Emails = null,
    IReadOnlyList<ContactPhoneInput>? Phones = null);

/// <param name="Kind">
/// Office, fax or mobile. <b>Optional, and a null is passed through rather than
/// filled in</b> — a caller who does not know must not answer on the user's
/// behalf, which is the whole reason the column is nullable.
/// </param>
public sealed record ContactPhoneInput(string Number, ContactPhoneKind? Kind);
