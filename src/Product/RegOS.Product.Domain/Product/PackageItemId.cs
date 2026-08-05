using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Product;

public sealed class PackageItemId : StronglyTypedId
{
    public PackageItemId(Guid value) : base(value)
    {
    }

    public static PackageItemId New() => new(Guid.NewGuid());

    public static PackageItemId From(Guid value) => new(value);

    public static implicit operator Guid(PackageItemId id) => id.Value;
}
