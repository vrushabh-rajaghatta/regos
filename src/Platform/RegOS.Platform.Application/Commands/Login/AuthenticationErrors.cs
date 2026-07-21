namespace RegOS.Platform.Application.Commands.Login;

public static class AuthenticationErrors
{
    /// <summary>
    /// The only message sign-in ever returns. It names neither the email nor
    /// the reason, because distinguishing "no such account" from "wrong
    /// password" is what turns a login endpoint into an enumeration oracle
    /// (ADR-022).
    /// </summary>
    public const string InvalidCredentials =
        "Invalid email address or password.";
}
