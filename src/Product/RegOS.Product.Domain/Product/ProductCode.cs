using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Product;

/// <summary>
/// The identifier an organization uses for a product in its own regulatory
/// correspondence - on dossiers, in authority submissions and in labelling.
/// </summary>
/// <remarks>
/// Normalized to upper case because regulatory identifiers are case-insensitive
/// in practice: "abc-123" and "ABC-123" name the same product, and storing both
/// would let one product be registered twice. Normalizing here rather than at
/// the database means uniqueness holds regardless of collation.
/// </remarks>
public sealed class ProductCode : ValueObject
{
    public const int MaxLength = 50;

    private ProductCode(string value) => Value = value;

    public string Value { get; }

    public static ProductCode Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(ProductErrors.CodeRequired);

        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length > MaxLength)
            throw new DomainException(ProductErrors.CodeTooLong);

        // Deliberately permissive: letters, digits, dash and underscore covers
        // every authority format we have seen, and anything stricter would be
        // guessing at rules we cannot yet cite.
        if (!normalized.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_'))
            throw new DomainException(ProductErrors.CodeInvalidCharacters);

        return new ProductCode(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
