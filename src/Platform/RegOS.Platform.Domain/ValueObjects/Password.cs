using RegOS.SharedKernel.Exceptions;

namespace RegOS.Platform.Domain.ValueObjects;

/// <summary>
/// A password as the user typed it, validated but never stored. The plaintext
/// exists only long enough to be hashed; <see cref="UserCredential"/> holds the
/// hash and never sees this type.
///
/// Deliberately not a <c>ValueObject</c> with structural equality: comparing two
/// passwords for equality is not an operation this system should make easy.
/// </summary>
public sealed class Password
{
    public const int MinimumLength = 8;

    // PBKDF2 has no practical input limit, so this is a denial-of-service
    // guard rather than a cryptographic one: hashing a megabyte of text on an
    // unauthenticated endpoint is free work for an attacker.
    public const int MaximumLength = 256;

    private Password(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Password Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(PasswordErrors.Required);

        // No Trim: leading and trailing spaces are legitimate password
        // characters, and silently removing them would reject a password the
        // user successfully set.
        if (value.Length < MinimumLength)
            throw new DomainException(PasswordErrors.TooShort);

        if (value.Length > MaximumLength)
            throw new DomainException(PasswordErrors.TooLong);

        return new Password(value);
    }
}
