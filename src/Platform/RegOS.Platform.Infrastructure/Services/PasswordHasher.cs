using Microsoft.AspNetCore.Identity;

using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.ValueObjects;

using IdentityHasher = Microsoft.AspNetCore.Identity.PasswordHasher<object>;

namespace RegOS.Platform.Infrastructure.Services;

/// <summary>
/// A wrapper, and deliberately nothing more. Every cryptographic decision —
/// algorithm, salt, iteration count, encoding, and the versioned format that
/// makes upgrades possible — belongs to the framework implementation.
///
/// <see cref="IdentityHasher"/> is generic over a user type it never reads, so
/// a shared placeholder is passed rather than threading the aggregate through
/// the application layer for no purpose.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private static readonly object Unused = new();

    private readonly IdentityHasher _hasher = new();

    public string Hash(Password password)
        => _hasher.HashPassword(Unused, password.Value);

    public PasswordVerification Verify(
        string passwordHash,
        string providedPassword)
        => _hasher.VerifyHashedPassword(Unused, passwordHash, providedPassword)
            switch
            {
                PasswordVerificationResult.Success =>
                    PasswordVerification.Succeeded,

                PasswordVerificationResult.SuccessRehashNeeded =>
                    PasswordVerification.SucceededNeedsRehash,

                _ => PasswordVerification.Failed
            };
}
