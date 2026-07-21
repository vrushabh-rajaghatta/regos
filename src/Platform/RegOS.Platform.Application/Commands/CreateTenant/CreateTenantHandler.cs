using RegOS.Persistence;
using RegOS.Platform.Application.Invitations;
using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;
using RegOS.SharedKernel.Primitives;

using OrganizationAggregate =
    RegOS.Organization.Domain.Aggregates.Organization.Organization;
using OrganizationId =
    RegOS.Organization.Domain.Aggregates.Organization.OrganizationId;
using TenantAggregate = RegOS.Platform.Domain.Aggregates.Tenant.Tenant;
using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;

namespace RegOS.Platform.Application.Commands.CreateTenant;

/// <summary>
/// Provisions a customer: tenant, mirror organization, invited administrator.
/// Handler orchestration, per house style — no domain events, no triggers,
/// no interceptors for business rules; what happens when a tenant is created
/// is readable here and nowhere else.
/// </summary>
/// <remarks>
/// <para>
/// The mirror organization (ADR-032) shares the tenant's guid — the stated
/// convention for "this tenant's own company" — and carries the type the
/// platform administrator supplied. It lands in the new tenant's registry:
/// the first entry, and the default applicant for their filings.
/// </para>
/// <para>
/// The administrator arrives <c>Invited</c> with the tenant-administrator
/// role, and reaches <c>Active</c> only by accepting the invitation and
/// setting a password of their own (ADR-027). No credential is created here
/// and no password ever travels through provisioning.
/// </para>
/// <para>
/// Tenant, organization and user are staged and committed in one
/// <c>SaveChanges</c> — the same one-transaction shape as publish-and-
/// snapshot: a crash mid-provisioning must not leave a tenant without its
/// administrator. The invitation is issued last, outside that transaction,
/// because issuing includes the notification and its issuer already owns
/// that sequencing; if it fails, the tenant exists and the invitation can be
/// re-issued, which is a recoverable state — half a tenant is not.
/// </para>
/// </remarks>
public sealed class CreateTenantHandler
{
    private readonly RegOSDbContext _dbContext;
    private readonly InvitationIssuer _invitations;
    private readonly IUserPolicy _userPolicy;

    public CreateTenantHandler(
        RegOSDbContext dbContext,
        InvitationIssuer invitations,
        IUserPolicy userPolicy)
    {
        _dbContext = dbContext;
        _invitations = invitations;
        _userPolicy = userPolicy;
    }

    public async Task<CreateTenantResult> HandleAsync(
        CreateTenantCommand command,
        CancellationToken cancellationToken)
    {
        var email = Email.Create(command.AdminEmail);

        // Unscoped by tenant (ADR-021): the address must be free across all
        // of RegOS, and there is no tenant to scope by yet anyway.
        await _userPolicy.EnsureEmailIsUniqueAsync(email, cancellationToken);

        var tenant = TenantAggregate.Create(command.Name!);

        var mirror = OrganizationAggregate.Create(
            new OrganizationId(tenant.Id.Value),
            tenant.Id,
            tenant.Name,
            command.OrganizationType);

        var admin = UserAggregate.CreateForTenant(
            tenant.Id,
            email,
            command.AdminFirstName!,
            command.AdminLastName!,
            UserRole.TenantAdministrator);

        _dbContext.Tenants.Add(tenant);
        _dbContext.Organizations.Add(mirror);
        _dbContext.Users.Add(admin);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _invitations.IssueAsync(admin, DateTime.UtcNow, cancellationToken);

        return new CreateTenantResult(tenant.Id, admin.Id);
    }
}
