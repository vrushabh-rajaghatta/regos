using RegOS.Platform.Application.Authentication;

namespace RegOS.Api.Authentication;

/// <summary>
/// Writes and clears the two cookies a session is carried in.
/// </summary>
/// <remarks>
/// <para>
/// Both are <c>HttpOnly</c>, so no script can read them — which is the point of
/// moving off <c>localStorage</c>. An XSS flaw can still <em>use</em> the
/// session by making credentialed requests, but it can no longer exfiltrate the
/// tokens and replay them elsewhere, later, from another machine.
/// </para>
/// <para>
/// <b>Cookies bring CSRF risk that bearer headers do not</b>, because the
/// browser attaches them automatically. <c>SameSite=Strict</c> is the mitigation:
/// the cookies are simply not sent on requests originating from another site,
/// so a form or image on an attacker's page cannot act as the user. It is
/// stricter than <c>Lax</c> — a link from an external site into RegOS lands
/// signed out — and that is the right trade for an internal regulatory tool.
/// </para>
/// <para>
/// <c>Secure</c> is always on, including in development. Browsers make an
/// explicit exception for <c>localhost</c>, so this needs no environment
/// switch — and an environment switch is exactly how a cookie ends up sent in
/// clear text in production.
/// </para>
/// </remarks>
public static class SessionCookies
{
    public const string AccessToken = "regos_access";
    public const string RefreshToken = "regos_refresh";

    /// <summary>
    /// The refresh cookie is scoped to the endpoints that consume it, so it is
    /// not attached to every ordinary API call. Fewer requests carrying the
    /// long-lived secret means fewer places it can leak.
    /// </summary>
    private const string RefreshPath = "/api/auth";

    public static void Write(HttpResponse response, AuthenticatedSession session)
    {
        response.Cookies.Append(
            AccessToken,
            session.AccessToken,
            Options("/", session.AccessTokenExpiresAt));

        response.Cookies.Append(
            RefreshToken,
            session.RefreshToken,
            Options(RefreshPath, session.RefreshTokenExpiresAt));
    }

    /// <summary>
    /// Deletion must repeat the same path and flags the cookie was written
    /// with, or the browser removes nothing and the caller stays signed in.
    /// </summary>
    public static void Clear(HttpResponse response)
    {
        response.Cookies.Delete(AccessToken, Options("/", expiresAt: null));

        response.Cookies.Delete(
            RefreshToken, Options(RefreshPath, expiresAt: null));
    }

    private static CookieOptions Options(string path, DateTime? expiresAt) =>
        new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = path,
            Expires = expiresAt is null
                ? null
                : new DateTimeOffset(
                    DateTime.SpecifyKind(expiresAt.Value, DateTimeKind.Utc))
        };
}
