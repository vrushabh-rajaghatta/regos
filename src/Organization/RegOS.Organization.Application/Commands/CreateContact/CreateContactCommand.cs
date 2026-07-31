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
    IReadOnlyList<string>? Phones = null);
