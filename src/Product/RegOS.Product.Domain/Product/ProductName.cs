using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Product;

/// <summary>
/// A product's registered name. Normalized on creation so that " Ozempic " and
/// "Ozempic" are the same name rather than two.
/// </summary>
public sealed class ProductName : ValueObject
{
    public const int MaxLength = 200;

    private ProductName(string value) => Value = value;

    public string Value { get; }

    public static ProductName Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(ProductErrors.NameRequired);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException(ProductErrors.NameTooLong);

        return new ProductName(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
