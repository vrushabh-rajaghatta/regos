using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Product;

public sealed class PharmaceuticalProductDetailId : StronglyTypedId
{
    public PharmaceuticalProductDetailId(Guid value) : base(value)
    {
    }

    public static PharmaceuticalProductDetailId New() => new(Guid.NewGuid());

    public static PharmaceuticalProductDetailId From(Guid value) => new(value);

    public static implicit operator Guid(PharmaceuticalProductDetailId id)
        => id.Value;
}
