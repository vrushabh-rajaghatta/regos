using RegOS.Platform.Domain.ValueObjects;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;
using RegOS.Platform.Contracts;

namespace RegOS.Platform.Domain.Aggregates.User;

/// <summary>
/// A person who can access RegOS on behalf of a <see cref="TenantId"/> — or,
/// when the tenant is null, a person who operates the platform itself.
/// This is the business concept of a person — not an authentication account;
/// passwords, roles, permissions and sign-in are separate concerns owned
/// elsewhere.
/// </summary>
public sealed class User : AggregateRoot<UserId>
{
    private User(
        UserId id,
        TenantId? tenantId,
        UserRole role,
        Email email,
        string firstName,
        string lastName,
        DateTime createdOn)
    {
        Id = id;
        TenantId = tenantId;
        Role = role;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        Status = UserStatus.Invited;
        CreatedOn = createdOn;
    }

    /// <summary>
    /// Null for a platform user, never for a tenant user. The rule is not "a
    /// user may lack a tenant" — it is enforced per factory: only
    /// <see cref="CreatePlatformUser"/> can produce null, and
    /// <see cref="CreateForTenant"/> still rejects it. Every tenant-scoped
    /// query treats null as "not yours": a comparison against a caller's
    /// tenant can never match it.
    /// </summary>
    public TenantId? TenantId { get; private set; }

    /// <summary>
    /// What this user administers (ADR-033). Role and tenant agree by
    /// construction: only <see cref="CreatePlatformUser"/> produces
    /// <see cref="UserRole.PlatformAdministrator"/>, and it never has a
    /// tenant; <see cref="CreateForTenant"/> rejects the platform role.
    /// </summary>
    public UserRole Role { get; private set; }

    public Email Email { get; private set; }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public UserStatus Status { get; private set; }

    public DateTime CreatedOn { get; private set; }

    /// <summary>
    /// Invites a new tenant user: creates them in the
    /// <see cref="UserStatus.Invited"/> state, pending acceptance. The Email
    /// value object already guarantees a valid, normalized address; the
    /// aggregate enforces the remaining invariants (tenant present, names
    /// supplied).
    /// </summary>
    public static User CreateForTenant(
        TenantId tenantId,
        Email email,
        string firstName,
        string lastName,
        UserRole role = UserRole.Member)
    {
        if (tenantId is null)
            throw new DomainException(UserErrors.TenantRequired);

        // The platform role and a tenant are contradictory by definition
        // (ADR-033); rejecting the combination here keeps it unexpressible
        // rather than checkable.
        if (role == UserRole.PlatformAdministrator)
            throw new DomainException(UserErrors.PlatformRoleCannotBeTenantBound);

        return Create(tenantId, role, email, firstName, lastName);
    }

    /// <summary>
    /// Creates a person who operates the platform itself and belongs to no
    /// tenant. A separate factory rather than a nullable parameter on
    /// <see cref="CreateForTenant"/>, so "tenant user without a tenant" stays
    /// unexpressible: the only way to a null tenant is to ask for a platform
    /// user by name — and the role comes with it, never as a choice.
    /// </summary>
    public static User CreatePlatformUser(
        Email email,
        string firstName,
        string lastName)
        => Create(
            tenantId: null,
            UserRole.PlatformAdministrator,
            email,
            firstName,
            lastName);

    private static User Create(
        TenantId? tenantId,
        UserRole role,
        Email email,
        string firstName,
        string lastName)
    {
        if (email is null)
            throw new DomainException(UserErrors.EmailRequired);

        return new User(
            UserId.New(),
            tenantId,
            role,
            email,
            RequireName(firstName, UserErrors.FirstNameRequired),
            RequireName(lastName, UserErrors.LastNameRequired),
            DateTime.UtcNow);
    }

    /// <summary>Updates the user's name. No-op when the name is unchanged.</summary>
    public void ChangeName(string firstName, string lastName)
    {
        var newFirstName = RequireName(firstName, UserErrors.FirstNameRequired);
        var newLastName = RequireName(lastName, UserErrors.LastNameRequired);

        if (newFirstName == FirstName && newLastName == LastName)
            return;

        FirstName = newFirstName;
        LastName = newLastName;
    }

    /// <summary>Changes the user's email. No-op when the email is unchanged.</summary>
    public void ChangeEmail(Email email)
    {
        if (email is null)
            throw new DomainException(UserErrors.EmailRequired);

        if (email == Email)
            return;

        Email = email;
    }

    /// <summary>Invited/Inactive -> Active. Idempotent when already active.</summary>
    public void Activate()
    {
        if (Status == UserStatus.Active)
            return;

        Status = UserStatus.Active;
    }

    /// <summary>Active/Invited -> Inactive. Idempotent when already inactive.</summary>
    public void Deactivate()
    {
        if (Status == UserStatus.Inactive)
            return;

        Status = UserStatus.Inactive;
    }

    private static string RequireName(string value, string error)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(error);

        return value.Trim();
    }
}
