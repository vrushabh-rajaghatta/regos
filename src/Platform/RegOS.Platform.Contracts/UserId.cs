using RegOS.SharedKernel.Primitives;

namespace RegOS.Platform.Contracts;

/// <summary>
/// The identity of a RegOS user.
/// </summary>
/// <remarks>
/// <b>The one Platform type another bounded context may hold</b> (ADR-041).
/// Regulatory aggregates assign work to a person — a question's owner, a
/// commitment's owner — and need exactly three things from a user: a stable
/// identity, equality, and the ability to be absent. Not a name, not an email,
/// not permissions.
/// <para>
/// <b>It is not in the shared kernel, and the distinction matters.</b>
/// <c>TenantId</c> is there because ADR-030 argued the tenant had stopped being
/// one context's concept — every aggregate in RegOS is tenant-scoped. A user is
/// not an intrinsic property of every domain concept; it is the identity of an
/// aggregate that still belongs to Platform. ADR-017 rule 2 says the kernel must
/// know no bounded context, so ownership stays here and only the contract
/// crosses.
/// </para>
/// <para>
/// Holding this id never licenses navigating to a <c>User</c> (ES-014). A name
/// on a screen is a read model's job.
/// </para>
/// </remarks>
public sealed class UserId : StronglyTypedId
{
    public UserId(Guid value) : base(value)
    {
    }

    public static UserId New() => new(Guid.NewGuid());

    public static UserId From(Guid value) => new(value);

    public static implicit operator Guid(UserId id) => id.Value;
}
