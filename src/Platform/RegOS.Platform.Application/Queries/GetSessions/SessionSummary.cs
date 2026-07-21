namespace RegOS.Platform.Application.Queries.GetSessions;

/// <param name="UserAgent">
/// Raw, exactly as the browser sent it. Deliberately not parsed into "Chrome on
/// macOS": the moment RegOS interprets it, it owns the interpretation, and a
/// wrong guess about someone's device is worse than an ugly string they can
/// recognise (ADR-029).
/// </param>
/// <param name="IsCurrent">
/// Whether this is the session asking. Without it, "sign out everywhere else"
/// has no meaning a user can act on.
/// </param>
public sealed record SessionSummary(
    Guid Id,
    string? UserAgent,
    string? CreatedFromIp,
    DateTime CreatedOn,
    DateTime LastUsedOn,
    bool IsCurrent);
