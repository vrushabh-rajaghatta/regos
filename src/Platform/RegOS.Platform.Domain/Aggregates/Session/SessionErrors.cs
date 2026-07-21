namespace RegOS.Platform.Domain.Aggregates.Session;

public static class SessionErrors
{
    public const string UserRequired = "A session must belong to a user.";

    /// <summary>
    /// Answered for a session that does not exist and for one belonging to
    /// somebody else alike. Distinguishing them would confirm that a guessed id
    /// was real (ADR-022).
    /// </summary>
    public const string NotFound = "Session not found.";

    public const string ExpiryMustBeInTheFuture =
        "A session must expire after it is created.";
}
