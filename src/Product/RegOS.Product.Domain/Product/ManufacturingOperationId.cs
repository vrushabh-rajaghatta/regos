using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Product;

public sealed class ManufacturingOperationId : StronglyTypedId
{
    public ManufacturingOperationId(Guid value) : base(value)
    {
    }

    public static ManufacturingOperationId New() => new(Guid.NewGuid());

    public static ManufacturingOperationId From(Guid value) => new(value);

    public static implicit operator Guid(ManufacturingOperationId id) => id.Value;
}
