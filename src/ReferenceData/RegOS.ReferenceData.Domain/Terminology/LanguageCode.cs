using System.Diagnostics.CodeAnalysis;

using RegOS.SharedKernel.Exceptions;

namespace RegOS.ReferenceData.Domain.Terminology;

/// <summary>
/// The language a name is written in — an ISO 639-1 two-letter code, held as a
/// value rather than a reference-data row.
/// </summary>
/// <remarks>
/// <b>A fact about the world, not about a product</b>
/// (<see href="../../../../docs/adr/ADR-062-a-language-is-a-world-fact.md">ADR-062</see>).
/// It lived in <c>Product</c> until EPIC-022, because <c>TradeName</c> was the
/// only thing that needed it; <c>LocalLabel</c> was already reaching across a
/// context boundary for it, and <c>Country</c> became the third consumer.
/// <para>
/// <b>The move was predicted here, by the sentence below.</b> The original
/// docstring said a governed language model was premature because
/// <em>"countries drive validation… whereas language <b>currently</b> drives
/// display"</em>. EPIC-022 S003 makes a market's required languages a thing to
/// check against, so language now drives validation and a country answers it —
/// which is the condition that sentence named. The rule of three was the
/// weaker half of the argument; the prediction firing was the stronger one.
/// </para>
/// <para>
/// <b>Still a value object, and deliberately not a <see cref="CodedConcept"/>.</b>
/// <c>System</c> exists to record <em>whose word is this?</em>, and ISO 639 has
/// one authority RegOS is not going to swap — so the column would carry the same
/// constant forever. This type also validates its own shape, which a concept
/// drawn from a seeded list would not.
/// </para>
/// <para>
/// <b>Still not a governed table.</b> No aggregate branches on <em>which</em>
/// language, only on whether a market's required set is satisfied — and that
/// set is <c>Country</c>'s. The picker's readable names are a presentation list
/// (SC-105), not domain data.
/// <para>
/// It models the minimum demonstrated requirement, not the standard. If future
/// domain rules distinguish regional variants — <c>en-CA</c> from
/// <c>en-US</c> — this value object may evolve into a locale without changing
/// aggregate semantics, because no caller handles the raw string.
/// </para>
/// </remarks>
public sealed class LanguageCode : IEquatable<LanguageCode>
{
    public const int Length = 2;

    private LanguageCode(string value)
    {
        Value = value;
    }

    /// <summary>Lower-case, always. "EN" and "en" are the same language.</summary>
    public string Value { get; }

    /// <summary>
    /// The canonical way in. Trims, lower-cases, and rejects anything that is
    /// not two ASCII letters — so no caller ever holds an unvalidated code.
    /// </summary>
    public static LanguageCode Parse(string? value)
        => TryParse(value, out var code)
            ? code
            : throw new DomainException(
                value is null || value.Trim().Length == 0
                    ? LanguageCodeErrors.Required
                    : LanguageCodeErrors.NotRecognised);

    public static bool TryParse(
        string? value,
        [NotNullWhen(true)] out LanguageCode? code)
    {
        code = null;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        var trimmed = value.Trim();

        if (trimmed.Length != Length)
            return false;

        foreach (var character in trimmed)
        {
            if (!char.IsAsciiLetter(character))
                return false;
        }

        code = new LanguageCode(trimmed.ToLowerInvariant());

        return true;
    }

    /// <summary>
    /// Reads a code that is already known to be ISO 639-1 — a database column
    /// this type wrote. Named for what it asserts rather than for the
    /// conversion, so a caller cannot reach for it to skip validating input.
    /// </summary>
    public static LanguageCode FromIso639_1(string value) => Parse(value);

    public bool Equals(LanguageCode? other)
        => other is not null && Value == other.Value;

    public override bool Equals(object? obj)
        => obj is LanguageCode other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;

    public static bool operator ==(LanguageCode? left, LanguageCode? right)
        => Equals(left, right);

    public static bool operator !=(LanguageCode? left, LanguageCode? right)
        => !Equals(left, right);
}
