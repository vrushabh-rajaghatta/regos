using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Platform.Application.Commands.CreateTenant;

/// <summary>
/// Provisions a customer (ADR-030/ADR-032/ADR-033): the tenant, its mirror
/// organization, and its first administrator — invited, never passworded here.
/// </summary>
/// <remarks>
/// <see cref="OrganizationType"/> is an input because the platform
/// administrator filling in this form knows what the customer is; the mirror
/// organization's type is never guessed (ADR-032). This is also why this
/// command lives at the one place Platform legitimately references the
/// Organization context's domain: provisioning names a regulatory party,
/// exactly as a regulatory application names its applicant.
/// </remarks>
public sealed record CreateTenantCommand(
    string? Name,
    OrganizationType OrganizationType,
    string? AdminEmail,
    string? AdminFirstName,
    string? AdminLastName);
