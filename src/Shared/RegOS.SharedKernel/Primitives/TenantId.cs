namespace RegOS.SharedKernel.Primitives;

/// <summary>
/// Identifies a tenant — the isolation boundary every tenant-owned row is
/// scoped by.
/// </summary>
/// <remarks>
/// This is the one concrete id the kernel owns. Ids normally belong to the
/// bounded context that defines their aggregate, but the tenant is different:
/// <see cref="Abstractions.ITenantContext"/> already lives here, and every
/// context that stores tenant-owned data needs the same id type without
/// depending on another context to get it. Before this type existed the role
/// was played by <c>OrganizationId</c>, which coupled three domain projects to
/// the Organization context for what was really an infrastructure concern
/// (ADR-030).
/// </remarks>
public sealed class TenantId : StronglyTypedId
{
    public TenantId(Guid value) : base(value)
    {
    }

    public static TenantId New() => new(Guid.NewGuid());

    public static TenantId From(Guid value) => new(value);

    public static implicit operator Guid(TenantId id) => id.Value;
}
