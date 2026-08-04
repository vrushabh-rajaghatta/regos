using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Product;

/// <summary>
/// Identity for a pack's dated marketing status entry.
/// </summary>
/// <remarks>
/// <b>A sealed class, not a <c>record struct</c>, unlike every <c>*StatusEntry</c>
/// id shipped before it</b> (ADR-043, ES-020). Those fifteen are pending
/// migration and copying one would propagate it: a record-struct id cannot
/// satisfy <c>Entity&lt;TId&gt;</c>'s constraint, so its owner gets no identity
/// equality and no empty-guid guard. <c>IdentityConventionTests</c> enforces
/// this.
/// </remarks>
public sealed class PackageMarketingStatusEntryId : StronglyTypedId
{
    public PackageMarketingStatusEntryId(Guid value) : base(value)
    {
    }

    public static PackageMarketingStatusEntryId New() => new(Guid.NewGuid());

    public static PackageMarketingStatusEntryId From(Guid value) => new(value);

    public static implicit operator Guid(PackageMarketingStatusEntryId id)
        => id.Value;
}
