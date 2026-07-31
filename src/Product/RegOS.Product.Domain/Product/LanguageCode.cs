using System.Diagnostics.CodeAnalysis;

using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Domain.Product;

/// <summary>
/// The language a name is written in — an ISO 639-1 two-letter code, held as a
/// value rather than a reference-data row.
/// </summary>
/// <remarks>
/// <b>Nothing in the domain branches on language.</b> It participates in
/// identity — one trade name per (medicinal product, language) — but no rule
/// asks whether a name is French. That is what makes it a value and not an
/// enum, and what makes a governed <c>Language</c> table premature: countries
/// drive validation, authority selection and market identity, whereas language
/// currently drives display. The picker's readable names are a presentation
/// list (SC-105), not domain data.
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
                    ? MedicinalProductErrors.LanguageRequired
                    : MedicinalProductErrors.LanguageNotRecognised);

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
