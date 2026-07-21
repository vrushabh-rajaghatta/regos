using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Organization.Application.Commands.CreateOrganization;

/// <summary>
/// No tenant. Creating an organization is the one Organization operation that
/// cannot be tenant-scoped — it is what brings a tenant into existence, so
/// there is no organization to resolve it from. Every other tenant-scoped
/// command takes its organization from <c>ITenantContext</c> (ADR-013).
/// </summary>
public sealed record CreateOrganizationCommand(
    string? LegalName,
    OrganizationType Type);
