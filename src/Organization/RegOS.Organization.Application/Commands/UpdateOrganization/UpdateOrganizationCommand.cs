using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Organization.Application.Commands.UpdateOrganization;

/// <summary>
/// Data edits only. Status is not here: it belongs to Activate and Deactivate,
/// which are lifecycle transitions rather than corrections to a record.
/// </summary>
/// <remarks>
/// Identifiers are not here either, and for a different reason. A name is
/// corrected; an identifier is <em>issued</em> or <em>withdrawn</em> by a
/// registry, so it gets its own commands rather than arriving as one more field
/// in a form submit — where a dropped array would silently erase every
/// identifier the company holds.
/// </remarks>
public sealed record UpdateOrganizationCommand(
    OrganizationId Id,
    string? LegalName,
    OrganizationType Type,
    string? Acronym = null,
    string? NameNativeLanguage = null);
