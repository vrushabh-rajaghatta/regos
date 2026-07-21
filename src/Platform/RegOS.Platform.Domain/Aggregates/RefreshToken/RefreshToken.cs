using RegOS.Platform.Domain.Aggregates.User;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Platform.Domain.Aggregates.RefreshToken;

/// <summary>
/// A long-lived credential that buys a new access token without re-entering a
/// password.
///
/// Its own aggregate rather than state on <see cref="UserCredential"/>: a
/// credential answers "can this person sign in at all", and there is exactly
/// one per user forever. A refresh token answers "is this session still alive",
/// is created and destroyed repeatedly, and there will one day be several per
/// user at once. Different lifecycle, different aggregate.
///
/// Like <see cref="UserCredential"/>, this never sees the secret it represents.
/// It stores a hash produced by infrastructure, so a database disclosure yields
/// no usable sessions.
/// </summary>
public sealed class RefreshToken : AggregateRoot<RefreshTokenId>
{
    private RefreshToken()
    {
    }

    public UserId UserId { get; private set; } = default!;

    /// <summary>
    /// A hash of the token value. Never the value itself, for the same reason
    /// passwords are hashed — with the same consequence: RegOS cannot show a
    /// user their own token, only recognise it when presented.
    /// </summary>
    public string TokenHash { get; private set; } = default!;

    public DateTime ExpiresAt { get; private set; }

    public DateTime CreatedOn { get; private set; }

    /// <summary>Null while the token is live.</summary>
    public DateTime? RevokedOn { get; private set; }

    /// <summary>
    /// The token issued in this one's place when it was used. Rotation makes a
    /// stolen token detectable: if a revoked token is presented, either the
    /// legitimate client or the thief is replaying, and this records the chain
    /// that would let a later slice work out which.
    /// </summary>
    public RefreshTokenId? ReplacedBy { get; private set; }

    public static RefreshToken Issue(
        UserId userId,
        string tokenHash,
        DateTime expiresAt,
        DateTime now)
    {
        if (userId is null)
            throw new DomainException(RefreshTokenErrors.UserRequired);

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new DomainException(RefreshTokenErrors.TokenHashRequired);

        if (expiresAt <= now)
            throw new DomainException(
                RefreshTokenErrors.ExpiryMustBeInTheFuture);

        return new RefreshToken
        {
            Id = RefreshTokenId.New(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedOn = now
        };
    }

    /// <summary>
    /// Whether this token can still be exchanged. Expiry and revocation are
    /// asked as one question so no caller can check half of it.
    /// </summary>
    public bool IsActiveAt(DateTime now) =>
        RevokedOn is null && now < ExpiresAt;

    /// <summary>
    /// Ends this token because a replacement was issued for it.
    /// </summary>
    public void RotateTo(RefreshTokenId replacement, DateTime now)
    {
        Revoke(now);

        ReplacedBy = replacement;
    }

    /// <summary>
    /// Ends this token without a replacement — signing out, or a later slice
    /// deciding a session is compromised.
    /// </summary>
    public void Revoke(DateTime now)
    {
        // Revoking twice is a no-op rather than an error: sign-out must be
        // idempotent, and a client retrying it has still achieved what it
        // asked for. The first revocation time is the true one and is kept.
        if (RevokedOn is not null) return;

        RevokedOn = now;
    }
}
