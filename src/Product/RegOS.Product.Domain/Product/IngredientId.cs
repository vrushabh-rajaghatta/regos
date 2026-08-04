using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Product;

public sealed class IngredientId : StronglyTypedId
{
    public IngredientId(Guid value) : base(value)
    {
    }

    public static IngredientId New() => new(Guid.NewGuid());

    public static IngredientId From(Guid value) => new(value);

    public static implicit operator Guid(IngredientId id) => id.Value;
}
