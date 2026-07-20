using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Product;

public sealed class ProductId : StronglyTypedId
{
    public ProductId(Guid value) : base(value)
    {
    }

    public static ProductId New() => new(Guid.NewGuid());

    public static ProductId From(Guid value) => new(value);

    public static implicit operator Guid(ProductId id) => id.Value;
}
