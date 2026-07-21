namespace RegOS.Platform.Domain.Aggregates.User;

/// <summary>
/// What a user is allowed to administer (ADR-033). Deliberately three values
/// and not a permission system: the pressure on authorization models is
/// always to grow matrices before any feature needs them.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Operates RegOS itself: creates and maintains tenants. Belongs to no
    /// tenant — the pairing is enforced by the factories, so a tenant-bound
    /// platform administrator is unexpressible.
    /// </summary>
    PlatformAdministrator = 1,

    /// <summary>
    /// Administers one tenant: invites and manages its users. The first user
    /// of every tenant, created by tenant provisioning.
    /// </summary>
    TenantAdministrator = 2,

    /// <summary>Does the regulatory work. The default for invited users.</summary>
    Member = 3
}
