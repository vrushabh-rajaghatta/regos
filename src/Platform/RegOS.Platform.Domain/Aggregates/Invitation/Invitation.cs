using RegOS.Platform.Domain.Aggregates.User;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Platform.Domain.Aggregates.Invitation;

/// <summary>
/// Permission to establish a user's first credential.
/// </summary>
/// <remarks>
/// <para>
/// Addressed to a <see cref="UserId"/> that already exists. An invited person is
/// a real <see cref="User"/> row from the moment they are invited (ADR-014, and
/// still true); this is not a second representation of them, it is a consumable
/// grant against the one that exists — which is why acceptance reconciles no
/// identities (ADR-027).
/// </para>
/// <para>
/// Like <see cref="UserCredential"/> and <see cref="RefreshToken"/>, it never
/// sees the secret it represents. It stores a hash, so a database disclosure
/// yields no usable invitations.
/// </para>
/// </remarks>
public sealed class Invitation : AggregateRoot<InvitationId>
{
    private Invitation()
    {
    }

    public UserId UserId { get; private set; } = default!;

    public string TokenHash { get; private set; } = default!;

    public DateTime ExpiresAt { get; private set; }

    public DateTime CreatedOn { get; private set; }

    /// <summary>Null until accepted. Set at most once.</summary>
    public DateTime? ConsumedOn { get; private set; }

    /// <summary>Null unless withdrawn, or replaced by a resend.</summary>
    public DateTime? RevokedOn { get; private set; }

    public static Invitation Issue(
        UserId userId,
        string tokenHash,
        DateTime expiresAt,
        DateTime now)
    {
        if (userId is null)
            throw new DomainException(InvitationErrors.UserRequired);

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new DomainException(InvitationErrors.TokenHashRequired);

        if (expiresAt <= now)
            throw new DomainException(
                InvitationErrors.ExpiryMustBeInTheFuture);

        return new Invitation
        {
            Id = InvitationId.New(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedOn = now
        };
    }

    /// <summary>
    /// Whether this invitation can still be accepted. Pending or finished, never
    /// both — expiry, consumption and revocation are asked as one question so no
    /// caller can check only part of it.
    /// </summary>
    public bool IsPendingAt(DateTime now) =>
        ConsumedOn is null && RevokedOn is null && now < ExpiresAt;

    /// <summary>
    /// Marks the invitation used. Throws rather than returning quietly: unlike
    /// revocation, consuming twice is never a legitimate retry — it means two
    /// acceptances raced, and the second must not be allowed to believe it won.
    /// </summary>
    public void Consume(DateTime now)
    {
        if (!IsPendingAt(now))
            throw new BusinessRuleViolationException(InvitationErrors.NotPending);

        ConsumedOn = now;
    }

    /// <summary>
    /// Withdraws the invitation, or retires it because a replacement was sent.
    /// </summary>
    public void Revoke(DateTime now)
    {
        // Idempotent, like RefreshToken.Revoke: withdrawing twice has achieved
        // what the caller asked. A consumed invitation is left alone — it was
        // used, and recording it as revoked would erase that.
        if (ConsumedOn is not null || RevokedOn is not null) return;

        RevokedOn = now;
    }
}
