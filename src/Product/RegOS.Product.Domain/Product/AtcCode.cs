using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Product;

/// <summary>
/// A WHO Anatomical Therapeutic Chemical code as the tenant supplied it —
/// <c>N02BE01</c>, <c>A10BA02</c>.
/// </summary>
/// <remarks>
/// <b>A type, and deliberately not a <c>CodedConcept</c>.</b> Storing this as
/// <c>("who-atc", "N02BE01", …)</c> would assert that WHO named it, and RegOS
/// holds no WHO ATC licence to check that against. The claim RegOS is entitled
/// to make is narrower: <em>the tenant told us this</em>. So it is its own
/// value object — strongly typed, but making no vocabulary claim (ADR-058 §6,
/// EPIC-010a D1).
/// <para>
/// The same call <c>Substance.UniiCode</c> got in S001, and for the same
/// reason: a field recorded as given is honest, a field dressed as licensed
/// terminology is not.
/// </para>
/// <para>
/// <b>Shape only, never membership.</b> The validation below checks that the
/// value looks like an ATC code; it cannot check that the code <em>exists</em>,
/// because that needs the vocabulary. When licensed terminology arrives, this
/// type gains a resolution step and every stored value can be checked against
/// it — which is why it is a type now rather than a bare string.
/// </para>
/// </remarks>
public sealed class AtcCode : ValueObject
{
    public const int MaxLength = 7;

    private AtcCode(string value) => Value = value;

    /// <summary>The code, upper-cased. <c>n02be01</c> and <c>N02BE01</c> are one code.</summary>
    public string Value { get; }

    /// <summary>
    /// The five ATC levels, most general first — <c>N</c>, <c>N02</c>,
    /// <c>N02B</c>, <c>N02BE</c>, <c>N02BE01</c>. A partial code yields only
    /// the levels it reaches.
    /// </summary>
    /// <remarks>
    /// Derived, never stored. It is what makes <em>"show me every analgesic"</em>
    /// a prefix match rather than a table of parent codes, and it is the reason
    /// the shape is worth validating at all.
    /// </remarks>
    public IReadOnlyList<string> Levels
    {
        get
        {
            int[] lengths = [1, 3, 4, 5, 7];

            return lengths
                .Where(length => Value.Length >= length)
                .Select(length => Value[..length])
                .ToList();
        }
    }

    /// <summary>
    /// The value is nullable on purpose: a market with no ATC code recorded is
    /// ordinary, and absence is not an error. Callers that mean "clear it" pass
    /// null and get null back.
    /// </summary>
    public static AtcCode? CreateOrNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : Create(value);

    public static AtcCode Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(AtcCodeErrors.Required);

        var normalized = value.Trim().ToUpperInvariant();

        if (!IsWellFormed(normalized))
            throw new DomainException(AtcCodeErrors.Malformed);

        return new AtcCode(normalized);
    }

    /// <summary>
    /// ATC is a fixed five-level alternation: letter · two digits · letter ·
    /// letter · two digits. A partial code is accepted — a class is a real
    /// answer when the product's own code has not been assigned — but a
    /// malformed one is not, because the level split above would be nonsense.
    /// </summary>
    private static bool IsWellFormed(string value)
    {
        if (value.Length is < 1 or > MaxLength || value.Length is 2 or 6)
            return false;

        // N 02 B E 01 — positions 0, 3, 4 are letters; 1-2 and 5-6 are digits.
        for (var i = 0; i < value.Length; i++)
        {
            var expectsDigit = i is 1 or 2 or 5 or 6;

            if (expectsDigit != char.IsAsciiDigit(value[i]))
                return false;

            if (!expectsDigit && !char.IsAsciiLetterUpper(value[i]))
                return false;
        }

        return true;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
