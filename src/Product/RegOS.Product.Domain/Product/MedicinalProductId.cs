using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Product;

public sealed class MedicinalProductId : StronglyTypedId
{
    public MedicinalProductId(Guid value) : base(value)
    {
    }

    public static MedicinalProductId New() => new(Guid.NewGuid());

    public static MedicinalProductId From(Guid value) => new(value);

    public static implicit operator Guid(MedicinalProductId id) => id.Value;
}
