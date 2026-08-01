using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Product;

public sealed class GlobalProductId : StronglyTypedId
{
    public GlobalProductId(Guid value) : base(value)
    {
    }

    public static GlobalProductId New() => new(Guid.NewGuid());

    public static GlobalProductId From(Guid value) => new(value);

    public static implicit operator Guid(GlobalProductId id) => id.Value;
}
