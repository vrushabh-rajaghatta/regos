using RegOS.Platform.Domain.Aggregates.User;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Platform.Domain.Aggregates.UserCredential;

/// <summary>
/// How a <see cref="User"/> proves who they are. Separate from the User
/// aggregate on purpose: a User is the business concept of a person, and
/// passwords, roles and sign-in are separate concerns.
///
/// Its identity <em>is</em> the <see cref="UserId"/>, which is what makes "at
/// most one credential per user" a property of the type rather than a rule
/// someone has to remember.
///
/// The aggregate never sees a plaintext password and never hashes anything. It
/// stores an opaque hash produced by the infrastructure, so the choice of
/// algorithm cannot leak into the domain.
/// </summary>
public sealed class UserCredential : AggregateRoot<UserId>
{
    private UserCredential()
    {
    }

    public string PasswordHash { get; private set; } = default!;

    public DateTime CreatedOn { get; private set; }

    public DateTime UpdatedOn { get; private set; }

    public static UserCredential Create(
        UserId userId,
        string passwordHash,
        DateTime now)
    {
        if (userId is null)
            throw new DomainException(UserCredentialErrors.UserRequired);

        return new UserCredential
        {
            Id = userId,
            PasswordHash = Validated(passwordHash),
            CreatedOn = now,
            UpdatedOn = now
        };
    }

    /// <summary>
    /// Replaces the stored hash. Used when a user changes their password, and
    /// later when a verified password needs rehashing because the algorithm's
    /// parameters have moved on.
    /// </summary>
    public void ChangePassword(string passwordHash, DateTime now)
    {
        PasswordHash = Validated(passwordHash);
        UpdatedOn = now;
    }

    private static string Validated(string passwordHash)
        => string.IsNullOrWhiteSpace(passwordHash)
            ? throw new DomainException(UserCredentialErrors.PasswordHashRequired)
            : passwordHash;
}
