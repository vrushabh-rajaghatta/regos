using System.Net;

using FluentAssertions;

using RegOS.Api.Authentication;

namespace RegOS.Api.Tests;

/// <summary>
/// The complete session lifecycle through the real ASP.NET pipeline.
///
/// This layer exists because neither of the others can reach it. Handler tests
/// never see a cookie, a middleware order or an authentication handler; browser
/// specs see the outcome but cannot present a deliberately stale token. Every
/// test here is about something that lives between the two.
/// </summary>
public sealed class SessionLifecycleTests
    : IClassFixture<RegOSApiFactory>, IAsyncLifetime
{
    private readonly RegOSApiFactory _factory;
    private readonly HttpClient _client;

    public SessionLifecycleTests(RegOSApiFactory factory)
    {
        _factory = factory;
        _client = factory.NewClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    /// <summary>
    /// Every test signs in, so every test leaves refresh tokens behind.
    /// Principle 7: a test owns what it creates.
    /// </summary>
    public async Task DisposeAsync()
    {
        await RefreshTokenStore.DeleteAllForAsync(Session.DevEmail);
    }

    private static string? CookieHeaderFor(
        HttpResponseMessage response, string name) =>
        response.Headers.TryGetValues("Set-Cookie", out var headers)
            ? headers.FirstOrDefault(h => h.StartsWith($"{name}="))
            : null;

    // ---------------------------------------------------------------- login

    [Fact]
    public async Task Login_sets_both_session_cookies()
    {
        var (session, response) = await Session.LoginAsync(_client);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        session.Access.Should().NotBeNullOrWhiteSpace();
        session.Refresh.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_returns_no_token_in_the_body()
    {
        // The whole point of moving to cookies: a script on the page must not
        // be able to read the token out of the response.
        var (_, response) = await Session.LoginAsync(_client);

        var body = await response.Content.ReadAsStringAsync();

        body.Should().BeEmpty();
    }

    [Theory]
    [InlineData(SessionCookies.AccessToken)]
    [InlineData(SessionCookies.RefreshToken)]
    public async Task Session_cookies_are_httponly_secure_and_samesite_strict(
        string cookie)
    {
        var (_, response) = await Session.LoginAsync(_client);

        var header = CookieHeaderFor(response, cookie);

        header.Should().NotBeNull();
        header.Should().Contain("httponly");
        header.Should().Contain("secure");
        header.Should().Contain("samesite=strict");
    }

    [Fact]
    public async Task The_refresh_cookie_is_scoped_to_the_auth_endpoints()
    {
        // So the long-lived secret is not attached to every ordinary API call.
        var (_, response) = await Session.LoginAsync(_client);

        CookieHeaderFor(response, SessionCookies.RefreshToken)
            .Should().Contain("path=/api/auth");
    }

    [Fact]
    public async Task Login_with_a_wrong_password_sets_no_cookies()
    {
        var (session, response) = await Session.LoginAsync(
            _client, password: "not the password");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        session.Access.Should().BeNull();
        session.Refresh.Should().BeNull();
    }

    // ------------------------------------------------------ the access cookie

    [Fact]
    public async Task The_access_cookie_authenticates_a_protected_endpoint()
    {
        var (session, _) = await Session.LoginAsync(_client);

        var response = await session.SendAsync(
            _client, HttpMethod.Get, "/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task A_protected_endpoint_rejects_a_request_with_no_cookie()
    {
        var response = await _client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task A_bearer_header_still_authenticates()
    {
        // Kept for non-browser callers. If this breaks, curl and the .http file
        // stop working and nothing else would notice.
        var (session, _) = await Session.LoginAsync(_client);

        var response = await Session.SendWithBearerAsync(
            _client, "/api/auth/me", session.Access!);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // -------------------------------------------------------------- refresh

    [Fact]
    public async Task Refresh_with_a_valid_cookie_issues_a_new_session()
    {
        var (session, _) = await Session.LoginAsync(_client);

        var before = session.Refresh;

        var response = await session.SendAsync(
            _client, HttpMethod.Post, "/api/auth/refresh");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Rotation: the token that comes back is not the one presented.
        session.Refresh.Should().NotBe(before);
    }

    [Fact]
    public async Task Refresh_with_no_cookie_is_rejected()
    {
        var response = await _client.PostAsync("/api/auth/refresh", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_with_an_unknown_token_is_rejected()
    {
        var response = await Session.SendWithCookieAsync(
            _client,
            HttpMethod.Post,
            "/api/auth/refresh",
            SessionCookies.RefreshToken,
            "a-token-that-was-never-issued");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task A_rotated_refresh_token_cannot_be_used_again()
    {
        var (session, _) = await Session.LoginAsync(_client);

        var original = session.Refresh!;

        await session.SendAsync(_client, HttpMethod.Post, "/api/auth/refresh");

        var replay = await Session.SendWithCookieAsync(
            _client,
            HttpMethod.Post,
            "/api/auth/refresh",
            SessionCookies.RefreshToken,
            original);

        replay.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Replaying_a_rotated_token_ends_the_whole_session()
    {
        // The security property of rotation. A replayed token means either the
        // legitimate client or a thief is out of step, and from here they are
        // indistinguishable — so every live token for that user is revoked and
        // both parties must sign in again.
        var (session, _) = await Session.LoginAsync(_client);

        var original = session.Refresh!;

        await session.SendAsync(_client, HttpMethod.Post, "/api/auth/refresh");

        var current = session.Refresh!;

        await Session.SendWithCookieAsync(
            _client,
            HttpMethod.Post,
            "/api/auth/refresh",
            SessionCookies.RefreshToken,
            original);

        // The token the honest client is holding is now dead too.
        var afterBreach = await Session.SendWithCookieAsync(
            _client,
            HttpMethod.Post,
            "/api/auth/refresh",
            SessionCookies.RefreshToken,
            current);

        afterBreach.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task An_expired_refresh_token_is_rejected()
    {
        var (session, _) = await Session.LoginAsync(_client);

        await RefreshTokenStore.ExpireAllForAsync(Session.DevEmail);

        var response = await session.SendAsync(
            _client, HttpMethod.Post, "/api/auth/refresh");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_stores_no_token_in_plaintext()
    {
        // Hashed at rest, like passwords. A database disclosure must not hand
        // over usable sessions.
        var (session, _) = await Session.LoginAsync(_client);

        var stored = await RefreshTokenStore.HashesForAsync(Session.DevEmail);

        stored.Should().NotBeEmpty();
        stored.Should().NotContain(session.Refresh!);
    }

    // --------------------------------------------------------------- logout

    [Fact]
    public async Task Logout_clears_both_cookies()
    {
        var (session, _) = await Session.LoginAsync(_client);

        var response = await session.SendAsync(
            _client, HttpMethod.Post, "/api/auth/logout");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        session.Access.Should().BeNull();
        session.Refresh.Should().BeNull();
    }

    [Fact]
    public async Task Logout_revokes_the_refresh_token()
    {
        var (session, _) = await Session.LoginAsync(_client);

        var refresh = session.Refresh!;

        await session.SendAsync(_client, HttpMethod.Post, "/api/auth/logout");

        // Not merely forgotten by the browser — the server refuses it, so a
        // token captured before sign-out is worthless afterwards.
        var response = await Session.SendWithCookieAsync(
            _client,
            HttpMethod.Post,
            "/api/auth/refresh",
            SessionCookies.RefreshToken,
            refresh);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_succeeds_with_no_session_at_all()
    {
        // Signing out must be safe to retry and must not reveal whether a token
        // existed.
        var response = await _client.PostAsync("/api/auth/logout", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task After_logout_the_browser_can_no_longer_reach_a_protected_endpoint()
    {
        var (session, _) = await Session.LoginAsync(_client);

        await session.SendAsync(_client, HttpMethod.Post, "/api/auth/logout");

        var response = await session.SendAsync(
            _client, HttpMethod.Get, "/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task An_access_token_captured_before_logout_still_works_until_it_expires()
    {
        // Stated as a test rather than left as a footnote, because it is the
        // honest limit of stateless tokens: logout revokes the refresh token
        // and clears the cookies, but it cannot un-sign a JWT. Anyone who
        // extracted the access token keeps it for the rest of its fifteen
        // minutes. If this ever starts failing, revocation has been added and
        // this test should be replaced rather than repaired.
        var (session, _) = await Session.LoginAsync(_client);

        var access = session.Access!;

        await session.SendAsync(_client, HttpMethod.Post, "/api/auth/logout");

        var response = await Session.SendWithBearerAsync(
            _client, "/api/auth/me", access);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
