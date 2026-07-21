using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Platform.Domain.Aggregates.Tenant;

/// <summary>
/// A customer of the platform — the boundary every tenant-owned record is
/// isolated by.
/// </summary>
/// <remarks>
/// Deliberately not an <c>Organization</c>. A tenant says <em>who pays for and
/// works in an isolated slice of RegOS</em>; an organization is a regulatory
/// party — an applicant, a manufacturer, a marketing authorization holder —
/// that a record can be about. The two coincided in one aggregate until
/// ADR-030 split them, which is why a tenant carries no regulatory taxonomy:
/// a <c>Type</c> like <c>MarketingAuthorizationHolder</c> describes a party,
/// not a customer account.
/// </remarks>
public sealed class Tenant : AggregateRoot<TenantId>
{
    private Tenant()
    {
    }

    public string Name { get; private set; } = default!;

    public TenantStatus Status { get; private set; }

    public static Tenant Create(TenantId id, string name)
        => new()
        {
            Id = id,
            Name = NormalizeName(name),
            Status = TenantStatus.Active
        };

    public static Tenant Create(string name)
        => Create(TenantId.New(), name);

    /// <summary>
    /// Corrects the tenant's name. Permitted while inactive: retiring a tenant
    /// says "no new work here", not "freeze the record", and a misspelled name
    /// is worth fixing either way.
    /// </summary>
    public void Rename(string? name)
        => Name = NormalizeName(name);

    /// <summary>
    /// Retires the tenant. Its data stays readable; deactivating says "no one
    /// signs in and nothing new starts here", not "pretend it never existed".
    /// </summary>
    public void Deactivate()
    {
        // Valid request, business state forbids it: 409, not a silent no-op
        // (ADR-009). A caller deactivating twice has a stale view of the world
        // and should be told so.
        if (Status == TenantStatus.Inactive)
            throw new BusinessRuleViolationException(TenantErrors.AlreadyInactive);

        Status = TenantStatus.Inactive;
    }

    /// <summary>
    /// Returns the tenant to service. The mirror of <see cref="Deactivate"/>,
    /// and rejected the same way when there is no transition to make.
    /// </summary>
    public void Activate()
    {
        if (Status == TenantStatus.Active)
            throw new BusinessRuleViolationException(TenantErrors.AlreadyActive);

        Status = TenantStatus.Active;
    }

    private static string NormalizeName(string? name)
        => string.IsNullOrWhiteSpace(name)
            ? throw new DomainException(TenantErrors.NameRequired)
            : name.Trim();
}
