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

    /// <summary>
    /// Every way an invitation can fail to be acceptable — unknown, expired,
    /// already used, withdrawn, or the account deactivated since. It names the
    /// two likely causes without saying which applies, because a 256-bit token
    /// cannot be enumerated and a user holding a dead link needs to know to ask
    /// for another one (ADR-027).
    /// </summary>
    public const string InvalidInvitation =
        "This invitation link is no longer valid. "
            + "It may have expired or already been used.";

    /// <summary>
    /// Every way a password reset can fail to be redeemable — unknown, expired,
    /// already used, superseded by a newer request, or the account deactivated
    /// since. Same reasoning as <see cref="InvalidInvitation"/>: the token is
    /// unguessable, so naming the likely causes enumerates nothing and tells a
    /// stuck user to ask again.
    /// </summary>
    public const string InvalidPasswordReset =
        "This password reset link is no longer valid. "
            + "It may have expired or already been used.";
}
