namespace RegOS.Platform.Domain.Aggregates.PasswordReset;

public static class PasswordResetErrors
{
    public const string UserRequired =
        "A password reset must belong to a user.";

    public const string TokenHashRequired =
        "A password reset token hash is required.";

    public const string ExpiryMustBeInTheFuture =
        "A password reset must expire after it is created.";

    /// <summary>
    /// Deliberately says nothing about <em>why</em>. The same sentence covers
    /// expired, already used and revoked, because a caller holding a reset link
    /// has not proved who they are and must not be told which.
    /// </summary>
    public const string NoLongerUsable =
        "This password reset link is no longer valid. Please request a new one.";
}
