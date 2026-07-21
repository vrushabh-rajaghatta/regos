using RegOS.Platform.Domain.ValueObjects;

namespace RegOS.Platform.Application.Services;

/// <summary>
/// Turns a password into an opaque hash, and checks one against the other.
///
/// The interface exists so the domain and application layers never name a
/// hashing library. RegOS writes no cryptography of its own — no salts, no
/// iteration counts, no bespoke formats — so the implementation is a thin wrapper
/// over a maintained framework primitive.
/// </summary>
public interface IPasswordHasher
{
    string Hash(Password password);

    PasswordVerification Verify(string passwordHash, string providedPassword);
}

public enum PasswordVerification
{
    Failed = 0,

    Succeeded = 1,

    /// <summary>
    /// The password was correct, but the stored hash used older parameters than
    /// the current ones. The caller should rehash and persist. Surfaced rather
    /// than hidden because silently discarding it is how a system stays on a
    /// weak algorithm for years.
    /// </summary>
    SucceededNeedsRehash = 2
}
