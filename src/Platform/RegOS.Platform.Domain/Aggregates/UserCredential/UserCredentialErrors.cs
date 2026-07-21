namespace RegOS.Platform.Domain.Aggregates.UserCredential;

public static class UserCredentialErrors
{
    public const string UserRequired =
        "A credential must belong to a user.";

    public const string PasswordHashRequired =
        "Password hash is required.";
}
