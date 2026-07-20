using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Platform.Domain.ValueObjects;

/// <summary>
/// A normalized, validated email address used to uniquely identify and
/// communicate with a person within the Platform. An email is defined by its
/// normalized value, so two addresses that differ only in letter casing or
/// surrounding whitespace are the same email.
/// </summary>
public sealed class Email : ValueObject
{
    private Email(string value) => Value = value;

    /// <summary>The normalized (trimmed, lower-cased) email address.</summary>
    public string Value { get; }

    /// <summary>
    /// Creates a valid, normalized <see cref="Email"/>.
    /// </summary>
    /// <remarks>
    /// The input is deliberately nullable. A null address is not treated as a
    /// caller/programming fault to be signalled with
    /// <see cref="ArgumentNullException"/>; it is simply one more invalid email
    /// value. Every invalid input — null, blank, or malformed — fails the same
    /// way: a <see cref="DomainException"/>, because forming an email address is
    /// a domain invariant, not an argument-passing contract.
    /// </remarks>
    public static Email Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(EmailErrors.Required);

        var normalized = value.Trim().ToLowerInvariant();

        if (!IsWellFormed(normalized))
            throw new DomainException(EmailErrors.Invalid);

        return new Email(normalized);
    }

    // Pragmatic, non-RFC validation (v1): no whitespace, exactly one '@', and a
    // non-empty local part and domain part. Deliberately not a full RFC 5322
    // parser — we tighten this only if a real regulatory rule demands it.
    private static bool IsWellFormed(string value)
    {
        if (value.Any(char.IsWhiteSpace))
            return false;

        var at = value.IndexOf('@');

        return at > 0                          // non-empty local part
               && at == value.LastIndexOf('@') // exactly one '@'
               && at < value.Length - 1;       // non-empty domain part
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
