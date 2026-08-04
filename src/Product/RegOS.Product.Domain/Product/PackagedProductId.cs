using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Product;

public sealed class PackagedProductId : StronglyTypedId
{
    public PackagedProductId(Guid value) : base(value)
    {
    }

    public static PackagedProductId New() => new(Guid.NewGuid());

    public static PackagedProductId From(Guid value) => new(value);

    public static implicit operator Guid(PackagedProductId id) => id.Value;
}
