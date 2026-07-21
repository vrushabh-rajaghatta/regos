using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Organization.Application.Commands.UpdateOrganization;

/// <summary>
/// Data edits only. Status is not here: it belongs to Activate and Deactivate,
/// which are lifecycle transitions rather than corrections to a record.
/// </summary>
public sealed record UpdateOrganizationCommand(
    OrganizationId Id,
    string? LegalName,
    OrganizationType Type);
