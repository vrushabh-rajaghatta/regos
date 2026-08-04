namespace RegOS.Platform.Application.Commands.CreateTenant;

/// <summary>
/// Provisions a customer (ADR-030/ADR-033): the tenant and its first
/// administrator — invited, never passworded here.
/// </summary>
/// <remarks>
/// No organization, and so no <c>OrganizationType</c> (ADR-060). Provisioning
/// names an account, not a regulatory party: the platform administrator
/// filling in this form knows who to invite, not what legal entity the
/// customer is. That is the tenant's own to record, afterwards, in its own
/// registry — which is why Platform no longer references the Organization
/// context's domain at all.
/// </remarks>
public sealed record CreateTenantCommand(
    string? Name,
    string? AdminEmail,
    string? AdminFirstName,
    string? AdminLastName);
