namespace RegOS.Platform.Domain.Aggregates.RefreshToken;

public static class RefreshTokenErrors
{
    public const string UserRequired =
        "A refresh token must belong to a user.";

    public const string SessionRequired =
        "A refresh token must belong to a session.";

    public const string TokenHashRequired =
        "A refresh token hash is required.";

    public const string ExpiryMustBeInTheFuture =
        "A refresh token must expire after it is created.";

    public const string AlreadyRevoked =
        "This refresh token has already been revoked.";
}
