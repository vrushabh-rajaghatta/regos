using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Organization.Application.Commands.CreateOrganization;

/// <summary>
/// No tenant property, like every other tenant-scoped command: the owning
/// tenant is ambient, resolved from <c>ITenantContext</c> (ADR-013). This
/// command once carried a doc comment claiming it could not be tenant-scoped
/// because creating an organization was what brought a tenant into existence —
/// true under the fused model, retired by ADR-030/ADR-032.
/// </summary>
public sealed record CreateOrganizationCommand(
    string? LegalName,
    OrganizationType Type);
