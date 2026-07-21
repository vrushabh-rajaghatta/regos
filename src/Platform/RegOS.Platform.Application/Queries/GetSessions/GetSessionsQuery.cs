namespace RegOS.Platform.Application.Queries.GetSessions;

/// <param name="RefreshToken">
/// The caller's refresh cookie, used only to mark which session is theirs. The
/// user id comes from the access token, never from here.
/// </param>
public sealed record GetSessionsQuery(string? RefreshToken);
