using RegOS.Platform.Domain.Aggregates.User;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Platform.Domain.Aggregates.Session;

/// <summary>
/// One continuous period of a user being signed in, on one device.
/// </summary>
/// <remarks>
/// <para>
/// The inversion AUTH-010 exists to make. Refresh tokens came first and a
/// "session" was whatever one of them implied; now a session is the thing that
/// exists, and refresh tokens are how it is carried. A user can say "sign out
/// that laptop" because the laptop is a row, rather than "revoke token 8f3b…",
/// which is a sentence nobody should have to say.
/// </para>
/// <para>
/// Crucially the session survives rotation. Every refresh mints a new token and
/// revokes the old one, so a working day produces roughly thirty-two token rows
/// — but one session row, whose <see cref="Id"/> never changes. Without that,
/// the sessions list would show thirty-two entries for one browser and offer to
/// revoke ids that stop existing fifteen minutes later.
/// </para>
/// <para>
/// It also owns the device context, which is stored once here rather than
/// copied onto every rotation.
/// </para>
/// </remarks>
public sealed class Session : AggregateRoot<SessionId>
{
    private Session()
    {
    }

    public UserId UserId { get; private set; } = default!;

    /// <summary>
    /// The browser's <c>User-Agent</c>, exactly as sent. Never parsed and never
    /// enriched: it exists so a person can recognise their own device in a
    /// list, not so RegOS can profile it (ADR-029).
    /// </summary>
    public string? UserAgent { get; private set; }

    /// <summary>
    /// The address the session was created from. Captured once, at sign-in, and
    /// never updated — it answers "where did this begin", not "where is this
    /// now", and the second question is one RegOS deliberately does not ask.
    /// </summary>
    public string? CreatedFromIp { get; private set; }

    public DateTime CreatedOn { get; private set; }

    /// <summary>
    /// Moved forward on every refresh. The one field that makes a stale session
    /// recognisable as stale.
    /// </summary>
    public DateTime LastUsedOn { get; private set; }

    /// <summary>
    /// When the session ends if it is not used. Carried here as well as on each
    /// token so that "is this session still alive" is answerable without
    /// loading the token chain.
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>Null while the session is live.</summary>
    public DateTime? RevokedOn { get; private set; }

    public static Session Start(
        UserId userId,
        string? userAgent,
        string? createdFromIp,
        DateTime expiresAt,
        DateTime now)
    {
        if (userId is null)
            throw new DomainException(SessionErrors.UserRequired);

        if (expiresAt <= now)
            throw new DomainException(SessionErrors.ExpiryMustBeInTheFuture);

        return new Session
        {
            Id = SessionId.New(),
            UserId = userId,
            // Truncated rather than rejected. A long or absent User-Agent is a
            // client's business, not a reason to refuse someone a session.
            UserAgent = Trimmed(userAgent, 512),
            CreatedFromIp = Trimmed(createdFromIp, 45),
            CreatedOn = now,
            LastUsedOn = now,
            ExpiresAt = expiresAt
        };
    }

    public bool IsActiveAt(DateTime now) => RevokedOn is null && now < ExpiresAt;

    /// <summary>
    /// Records that the session was just used, and extends it to match the
    /// replacement token's life.
    /// </summary>
    public void Refreshed(DateTime expiresAt, DateTime now)
    {
        LastUsedOn = now;
        ExpiresAt = expiresAt;
    }

    /// <summary>
    /// Ends the session. Idempotent, like every other revocation here: the
    /// first revocation time is the true one and is kept.
    /// </summary>
    public void Revoke(DateTime now)
    {
        if (RevokedOn is not null) return;

        RevokedOn = now;
    }

    private static string? Trimmed(string? value, int maximum) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Length <= maximum ? value : value[..maximum];
}
