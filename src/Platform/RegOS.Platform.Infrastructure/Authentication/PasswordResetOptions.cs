namespace RegOS.Platform.Infrastructure.Authentication;

/// <summary>
/// Password reset token settings, bound from the <c>PasswordReset</c>
/// configuration section.
/// </summary>
public sealed class PasswordResetOptions
{
    public const string SectionName = "PasswordReset";

    /// <summary>
    /// How long a reset link stays redeemable. One hour, against an
    /// invitation's seven days, and the difference is the point: an invitation
    /// is expected to sit in a mailbox until someone gets round to it, whereas
    /// a reset was requested seconds ago by someone waiting at the screen. A
    /// short window is the only defence against a mailbox that is read by
    /// somebody else.
    /// </summary>
    public int Minutes { get; set; } = 60;

    /// <summary>
    /// Where the "choose a new password" page lives. Used only to build the URL
    /// the notifier sends; the API never redirects there itself.
    /// </summary>
    public string CompleteUrl { get; set; } =
        "http://localhost:5173/reset-password";
}
