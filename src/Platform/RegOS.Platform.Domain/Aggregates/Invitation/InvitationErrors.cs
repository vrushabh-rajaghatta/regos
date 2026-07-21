namespace RegOS.Platform.Domain.Aggregates.Invitation;

public static class InvitationErrors
{
    public const string UserRequired =
        "An invitation must belong to a user.";

    public const string TokenHashRequired =
        "An invitation token hash is required.";

    public const string ExpiryMustBeInTheFuture =
        "An invitation must expire after it is issued.";

    /// <summary>
    /// Raised when consuming an invitation that is no longer pending. Not shown
    /// to whoever presented the token — acceptance answers every failure
    /// identically (ADR-022) — but it names the fault precisely for a caller
    /// that reached this state through a bug rather than a stale link.
    /// </summary>
    public const string NotPending =
        "This invitation is no longer pending.";
}
