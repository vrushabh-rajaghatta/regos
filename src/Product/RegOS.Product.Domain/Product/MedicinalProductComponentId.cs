using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Product;

public sealed class MedicinalProductComponentId : StronglyTypedId
{
    public MedicinalProductComponentId(Guid value) : base(value)
    {
    }

    public static MedicinalProductComponentId New() => new(Guid.NewGuid());

    public static MedicinalProductComponentId From(Guid value) => new(value);

    public static implicit operator Guid(MedicinalProductComponentId id)
        => id.Value;
}
