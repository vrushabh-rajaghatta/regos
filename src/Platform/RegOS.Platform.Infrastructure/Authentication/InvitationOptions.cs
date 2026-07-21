namespace RegOS.Platform.Infrastructure.Authentication;

/// <summary>
/// Invitation token settings, bound from the <c>Invitation</c> configuration
/// section.
/// </summary>
public sealed class InvitationOptions
{
    public const string SectionName = "Invitation";

    /// <summary>
    /// How long an invitation stays acceptable. Seven days: long enough to
    /// survive a holiday, short enough that a mailbox compromised months later
    /// is not an account takeover. ADR-014 accepted "never expires"; ADR-027
    /// reversed it.
    /// </summary>
    public int Days { get; set; } = 7;

    /// <summary>
    /// Where the acceptance page lives. Used only to build the URL the notifier
    /// sends; the API never redirects there itself.
    /// </summary>
    public string AcceptUrl { get; set; } =
        "http://localhost:5173/accept-invitation";
}
