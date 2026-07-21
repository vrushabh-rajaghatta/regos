using RegOS.Platform.Domain.Aggregates.User;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Platform.Domain.Aggregates.PasswordReset;

/// <summary>
/// Permission, granted once and for a short time, to replace a forgotten
/// password without proving the old one.
///
/// Its own aggregate rather than state on <see cref="UserCredential"/>: a
/// credential is what the user has, and there is one per user forever. A reset
/// is a request the user made, several of which may have been made and
/// abandoned over the years. Recording them on the credential would mean the
/// credential changed every time someone merely asked.
///
/// Like every other grant here, it never sees the secret it represents. It
/// stores a hash produced by infrastructure, so a database disclosure yields
/// no usable reset links.
/// </summary>
public sealed class PasswordReset : AggregateRoot<PasswordResetId>
{
    private PasswordReset()
    {
    }

    public UserId UserId { get; private set; } = default!;

    /// <summary>
    /// A hash of the token value that was sent to the user. Never the value —
    /// so RegOS can recognise a reset link when one is presented, but could not
    /// reconstruct one for anybody, including itself.
    /// </summary>
    public string TokenHash { get; private set; } = default!;

    public DateTime ExpiresAt { get; private set; }

    public DateTime CreatedOn { get; private set; }

    /// <summary>
    /// When the reset was actually used to set a new password. Null until then,
    /// and set exactly once: this is what makes the grant single-use.
    /// </summary>
    public DateTime? ConsumedOn { get; private set; }

    /// <summary>
    /// When the reset was withdrawn without being used — because a newer one
    /// replaced it, or the account was closed.
    /// </summary>
    public DateTime? RevokedOn { get; private set; }

    public static PasswordReset Issue(
        UserId userId,
        string tokenHash,
        DateTime expiresAt,
        DateTime now)
    {
        if (userId is null)
            throw new DomainException(PasswordResetErrors.UserRequired);

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new DomainException(PasswordResetErrors.TokenHashRequired);

        if (expiresAt <= now)
            throw new DomainException(
                PasswordResetErrors.ExpiryMustBeInTheFuture);

        return new PasswordReset
        {
            Id = PasswordResetId.New(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedOn = now
        };
    }

    /// <summary>
    /// Whether this reset can still be redeemed. Unused, unwithdrawn and
    /// unexpired are asked as one question so that no caller can check two
    /// of the three and believe it has checked the grant.
    /// </summary>
    public bool IsUsableAt(DateTime now) =>
        ConsumedOn is null && RevokedOn is null && now < ExpiresAt;

    /// <summary>
    /// Spends the reset. Unlike revocation this is not idempotent: a second
    /// attempt on the same link is either a duplicate submission or a replay,
    /// and from inside the domain those are indistinguishable, so both are
    /// refused rather than quietly accepted.
    /// </summary>
    public void Consume(DateTime now)
    {
        if (!IsUsableAt(now))
            throw new BusinessRuleViolationException(
                PasswordResetErrors.NoLongerUsable);

        ConsumedOn = now;
    }

    /// <summary>
    /// Withdraws the reset before it is used.
    /// </summary>
    public void Revoke(DateTime now)
    {
        // Two no-ops rather than errors. Revoking twice must be safe, because
        // issuing a replacement withdraws whatever came before without first
        // asking what state it was in. And a reset that was already spent is
        // history: overwriting it with a revocation would erase the record of
        // the password actually having been changed.
        if (ConsumedOn is not null || RevokedOn is not null) return;

        RevokedOn = now;
    }
}
