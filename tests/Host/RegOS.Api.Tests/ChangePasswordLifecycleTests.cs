using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using RegOS.Api.Authentication;
using RegOS.Platform.Contracts;

namespace RegOS.Api.Tests;

/// <summary>
/// Changing your own password, through the real pipeline.
///
/// Each test owns the account it changes: it invites a user, accepts on their
/// behalf, and deletes them afterwards (ADR-019 principle 7). Changing the
/// shared development account's password would break every other test in the
/// run — and, since the change revokes sessions, would do so halfway through.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class ChangePasswordLifecycleTests : IAsyncLifetime
{
    private const string Marker = "changepasswordtest";
    private const string OriginalPassword = "the original password";
    private const string NewPassword = "a brand new password";

    private readonly RegOSApiFactory _factory;
    private readonly HttpClient _client;

    private Session _admin = default!;

    public ChangePasswordLifecycleTests(RegOSApiFactory factory)
    {
        _factory = factory;
        _client = factory.NewClient();
    }

    public async Task InitializeAsync()
    {
        (_admin, _) = await Session.LoginAsync(_client);
    }

    public async Task DisposeAsync()
    {
        await _factory.RefreshTokens.DeleteAllForAsync(Session.DevEmail);
        await _factory.Users.DeleteUsersMatchingAsync(Marker);
    }

    /// <summary>Creates an active account this test owns, and signs it in.</summary>
    private async Task<(string Email, Guid UserId, Session Session)>
        NewSignedInUserAsync()
    {
        var email = $"{Marker}.{Guid.NewGuid():N}@policy.example";

        var invited = await _admin.SendJsonAsync(
            _client,
            HttpMethod.Post,
            "/api/platform/users/invitations",
            new { firstName = "Change", lastName = "Password", email });

        invited.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await invited.Content
            .ReadFromJsonAsync<Dictionary<string, object>>();

        var userId = Guid.Parse(created!["id"].ToString()!);

        var accepted = await _client.PostAsJsonAsync(
            "/api/auth/invitations/accept",
            new
            {
                token = _factory.Invitations.TokenFor(email),
                password = OriginalPassword
            });

        accepted.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var (session, login) = await Session.LoginAsync(
            _client, email, OriginalPassword);

        login.StatusCode.Should().Be(HttpStatusCode.NoContent);

        return (email, userId, session);
    }

    private Task<HttpResponseMessage> ChangeAsync(
        Session session,
        string currentPassword = OriginalPassword,
        string newPassword = NewPassword) =>
        session.SendJsonAsync(
            _client,
            HttpMethod.Post,
            "/api/auth/change-password",
            new { currentPassword, newPassword });

    // ------------------------------------------------- the whole invariant

    [Fact]
    public async Task Changing_a_password_ends_every_way_the_old_one_was_trusted()
    {
        // ADR-028, demonstrated end to end in one place. Everything below this
        // test checks one clause of it; this one checks that they hold together.
        var (email, _, session) = await NewSignedInUserAsync();

        var refreshBefore = session.Refresh!;

        // An outstanding reset link, as if someone had used "forgot password".
        (await _client.PostAsJsonAsync(
            "/api/auth/password-reset/request", new { email }))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var resetLink = _factory.PasswordResets.TokenFor(email);

        var response = await ChangeAsync(session);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 1. The old password is gone.
        (await Session.LoginAsync(_client, email, OriginalPassword))
            .Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // 2. The new one works.
        (await Session.LoginAsync(_client, email, NewPassword))
            .Response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 3. The session that made the request is dead, including its own.
        (await Session.SendWithCookieAsync(
            _client,
            HttpMethod.Post,
            "/api/auth/refresh",
            SessionCookies.RefreshToken,
            refreshBefore))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // 4. The reset link somebody else may be holding is dead too.
        (await _client.PostAsJsonAsync(
            "/api/auth/password-reset/complete",
            new { token = resetLink, password = "attacker chosen password" }))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // 5. And that link really did not take effect.
        (await Session.LoginAsync(_client, email, NewPassword))
            .Response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // ---------------------------------------------------------------- cookies

    [Fact]
    public async Task Changing_a_password_clears_both_cookies()
    {
        // Without this the browser keeps an access cookie that works for the
        // rest of its fifteen minutes while the refresh behind it is revoked -
        // a half-signed-in state that reads as a bug.
        var (_, _, session) = await NewSignedInUserAsync();

        await ChangeAsync(session);

        session.Access.Should().BeNull();
        session.Refresh.Should().BeNull();
    }

    [Fact]
    public async Task An_access_token_captured_before_the_change_still_works_until_it_expires()
    {
        // The same honest limit as logout, stated rather than left implicit: a
        // JWT is a signed statement, not a database row. If this ever starts
        // failing, revocation has been added and this test should be replaced
        // rather than repaired.
        var (_, _, session) = await NewSignedInUserAsync();

        var access = session.Access!;

        await ChangeAsync(session);

        var response = await Session.SendWithBearerAsync(
            _client, "/api/auth/me", access);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ------------------------------------------------------------ refusals

    [Fact]
    public async Task Requires_a_session()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/change-password",
            new { currentPassword = OriginalPassword, newPassword = NewPassword });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refuses_an_incorrect_current_password()
    {
        var (email, _, session) = await NewSignedInUserAsync();

        var response = await ChangeAsync(session, currentPassword: "wrong");

        // 400, not 401. The caller is authenticated and permitted to do this;
        // what is wrong is a field. Answering 401 told our own client to
        // refresh and replay, and a browser spec caught it signing the user
        // out over a typo (ADR-028).
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();

        // Named plainly. The caller is authenticated, so there is nothing left
        // to enumerate and the uniform-message rule buys nothing here.
        body.Should().Contain("current password is incorrect");

        (await Session.LoginAsync(_client, email, OriginalPassword))
            .Response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Refuses_a_new_password_that_breaks_the_rules()
    {
        var (email, _, session) = await NewSignedInUserAsync();

        var response = await ChangeAsync(session, newPassword: "short");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Nothing half-applied: the old password still works and the session
        // that asked is still alive.
        (await Session.LoginAsync(_client, email, OriginalPassword))
            .Response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task A_refused_change_revokes_nothing()
    {
        // Otherwise a stolen access token alone would let someone sign the real
        // user out of everything without knowing any password.
        var (_, _, session) = await NewSignedInUserAsync();

        var refresh = session.Refresh!;

        await ChangeAsync(session, currentPassword: "wrong");

        var stillAlive = await Session.SendWithCookieAsync(
            _client,
            HttpMethod.Post,
            "/api/auth/refresh",
            SessionCookies.RefreshToken,
            refresh);

        stillAlive.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task A_user_can_only_change_their_own_password()
    {
        // Not by a check, but by construction: the command has nowhere to put
        // someone else's id. This test exists to notice if that ever changes -
        // if a UserId is added to the request, the admin's password would be
        // the one at risk, and this would still pass silently unless someone
        // asserts the shape.
        var (email, _, session) = await NewSignedInUserAsync();

        var response = await session.SendJsonAsync(
            _client,
            HttpMethod.Post,
            "/api/auth/change-password",
            new
            {
                userId = Guid.NewGuid(),
                currentPassword = OriginalPassword,
                newPassword = NewPassword
            });

        // The extra field is ignored, and the caller's own password changed.
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await Session.LoginAsync(_client, email, NewPassword))
            .Response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
