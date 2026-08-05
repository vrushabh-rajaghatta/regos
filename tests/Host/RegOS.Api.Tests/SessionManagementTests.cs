using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using RegOS.Api.Authentication;

namespace RegOS.Api.Tests;

/// <summary>
/// Sessions as a thing a user owns and can end.
///
/// Each test invites and accepts its own account, so it can sign in several
/// times without disturbing anyone else's sessions, and deletes it afterwards
/// (ADR-019 principle 7).
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class SessionManagementTests : IAsyncLifetime
{
    private const string Marker = "sessiontest";
    private const string Password = "the account password";

    private readonly RegOSApiFactory _factory;
    private readonly HttpClient _client;

    private Session _admin = default!;

    public SessionManagementTests(RegOSApiFactory factory)
    {
        _factory = factory;
        _client = factory.NewClient();
    }

    public async Task InitializeAsync() =>
        (_admin, _) = await Session.LoginAsync(_client);

    public async Task DisposeAsync()
    {
        await _factory.RefreshTokens.DeleteAllForAsync(Session.DevEmail);
        await _factory.Users.DeleteUsersMatchingAsync(Marker);
    }

    private sealed record SessionRow(
        Guid Id,
        string? UserAgent,
        string? CreatedFromIp,
        DateTime CreatedOn,
        DateTime LastUsedOn,
        bool IsCurrent);

    private async Task<string> NewAccountAsync()
    {
        var email = $"{Marker}.{Guid.NewGuid():N}@policy.example";

        var invited = await _admin.SendJsonAsync(
            _client,
            HttpMethod.Post,
            "/api/platform/users/invitations",
            new { firstName = "Session", lastName = "Owner", email });

        invited.StatusCode.Should().Be(HttpStatusCode.Created);

        var accepted = await _client.PostAsJsonAsync(
            "/api/auth/invitations/accept",
            new { token = _factory.Invitations.TokenFor(email), password = Password });

        accepted.StatusCode.Should().Be(HttpStatusCode.NoContent);

        return email;
    }

    private async Task<Session> SignInAsync(string email)
    {
        var (session, response) = await Session.LoginAsync(_client, email, Password);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        return session;
    }

    private async Task<IReadOnlyList<SessionRow>> ListAsync(Session session)
    {
        var response = await session.SendAsync(
            _client, HttpMethod.Get, "/api/auth/sessions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return (await response.Content
            .ReadFromJsonAsync<List<SessionRow>>())!;
    }

    // ------------------------------------------------------------- listing

    [Fact]
    public async Task Signing_in_three_times_produces_three_sessions()
    {
        var email = await NewAccountAsync();

        await SignInAsync(email);
        await SignInAsync(email);
        var third = await SignInAsync(email);

        var sessions = await ListAsync(third);

        sessions.Should().HaveCount(3);
    }

    [Fact]
    public async Task Refreshing_does_not_create_a_second_session()
    {
        // The reason a session exists as its own row. Rotation mints a new
        // refresh token every fifteen minutes; if the list were built from
        // tokens, a working day would show dozens of "devices".
        var email = await NewAccountAsync();
        var session = await SignInAsync(email);

        var id = (await ListAsync(session)).Single().Id;

        await session.SendAsync(_client, HttpMethod.Post, "/api/auth/refresh");
        await session.SendAsync(_client, HttpMethod.Post, "/api/auth/refresh");

        var after = await ListAsync(session);

        after.Should().HaveCount(1);
        after.Single().Id.Should().Be(id);
    }

    [Fact]
    public async Task Refreshing_moves_last_used_forward()
    {
        var email = await NewAccountAsync();
        var session = await SignInAsync(email);

        var before = (await ListAsync(session)).Single().LastUsedOn;

        await Task.Delay(1100);

        await session.SendAsync(_client, HttpMethod.Post, "/api/auth/refresh");

        var after = (await ListAsync(session)).Single().LastUsedOn;

        after.Should().BeAfter(before);
    }

    [Fact]
    public async Task A_session_records_the_device_that_started_it()
    {
        var email = await NewAccountAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { email, password = Password })
        };

        request.Headers.Add("User-Agent", "RegOS-Spec/1.0");

        var response = await _client.SendAsync(request);

        var session = new Session();
        session.Absorb(response);

        var row = (await ListAsync(session)).Single();

        // Raw, unparsed, exactly as sent (ADR-029).
        row.UserAgent.Should().Be("RegOS-Spec/1.0");

        // The address is deliberately not asserted. WebApplicationFactory hosts
        // the app in-process with no socket behind it, so RemoteIpAddress is
        // genuinely null here - a limit of this test layer rather than of the
        // feature. Verified by hand against the running API instead, and
        // recorded with SEC-001, since behind a proxy it would be the proxy's
        // address until UseForwardedHeaders exists.
    }

    [Fact]
    public async Task The_caller_can_tell_which_session_is_theirs()
    {
        var email = await NewAccountAsync();

        await SignInAsync(email);
        var mine = await SignInAsync(email);

        var sessions = await ListAsync(mine);

        sessions.Where(x => x.IsCurrent).Should().HaveCount(1);
    }

    [Fact]
    public async Task A_user_sees_only_their_own_sessions()
    {
        var email = await NewAccountAsync();
        var session = await SignInAsync(email);

        // The administrator is signed in too, on a different account.
        var sessions = await ListAsync(session);

        sessions.Should().HaveCount(1);
    }

    // ------------------------------------------------------------ revoking

    [Fact]
    public async Task Revoking_a_session_ends_it()
    {
        var email = await NewAccountAsync();

        var other = await SignInAsync(email);
        var mine = await SignInAsync(email);

        var target = (await ListAsync(mine)).Single(x => !x.IsCurrent);

        var revoked = await mine.SendAsync(
            _client, HttpMethod.Delete, $"/api/auth/sessions/{target.Id}");

        revoked.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Gone from the list...
        (await ListAsync(mine)).Should().HaveCount(1);

        // ...and genuinely dead, not merely hidden.
        var refresh = await Session.SendWithCookieAsync(
            _client,
            HttpMethod.Post,
            "/api/auth/refresh",
            SessionCookies.RefreshToken,
            other.Refresh!);

        refresh.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Revoking_your_own_current_session_signs_you_out()
    {
        var email = await NewAccountAsync();
        var mine = await SignInAsync(email);

        var current = (await ListAsync(mine)).Single(x => x.IsCurrent);

        var revoked = await mine.SendAsync(
            _client, HttpMethod.Delete, $"/api/auth/sessions/{current.Id}");

        revoked.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // The cookies went with it, rather than leaving the browser presenting
        // a session the server has ended.
        mine.Access.Should().BeNull();
        mine.Refresh.Should().BeNull();
    }

    [Fact]
    public async Task Revoking_everything_else_keeps_the_session_that_asked()
    {
        // "Sign out my other devices" - the capability AUTH-009 deferred
        // because it had no vocabulary for "else".
        var email = await NewAccountAsync();

        var first = await SignInAsync(email);
        var second = await SignInAsync(email);
        var mine = await SignInAsync(email);

        var response = await mine.SendAsync(
            _client, HttpMethod.Post, "/api/auth/sessions/revoke-others");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var remaining = await ListAsync(mine);

        remaining.Should().HaveCount(1);
        remaining.Single().IsCurrent.Should().BeTrue();

        foreach (var dead in new[] { first.Refresh!, second.Refresh! })
        {
            (await Session.SendWithCookieAsync(
                _client,
                HttpMethod.Post,
                "/api/auth/refresh",
                SessionCookies.RefreshToken,
                dead))
                .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    [Fact]
    public async Task A_user_cannot_revoke_somebody_elses_session()
    {
        // The whole security content of the endpoint: the id is a guid the
        // caller supplies, so ownership must be proven rather than assumed.
        var email = await NewAccountAsync();
        var mine = await SignInAsync(email);

        var adminSession = (await ListAsync(_admin)).First(x => x.IsCurrent);

        var response = await mine.SendAsync(
            _client, HttpMethod.Delete, $"/api/auth/sessions/{adminSession.Id}");

        // 404, not 403: "that is not yours" would confirm the id was real.
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // And the administrator is still signed in.
        (await _admin.SendAsync(_client, HttpMethod.Get, "/api/auth/me"))
            .StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task An_unknown_session_is_not_found()
    {
        var email = await NewAccountAsync();
        var mine = await SignInAsync(email);

        var response = await mine.SendAsync(
            _client, HttpMethod.Delete, $"/api/auth/sessions/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Sessions_require_authentication()
    {
        (await _client.GetAsync("/api/auth/sessions"))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        (await _client.PostAsync("/api/auth/sessions/revoke-others", null))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -------------------------------------------------- the other exits

    [Fact]
    public async Task Signing_out_ends_the_session_not_merely_the_token()
    {
        var email = await NewAccountAsync();

        var going = await SignInAsync(email);
        var staying = await SignInAsync(email);

        (await ListAsync(staying)).Should().HaveCount(2);

        await going.SendAsync(_client, HttpMethod.Post, "/api/auth/logout");

        // Otherwise the browser that signed out would still be sitting on the
        // user's own sessions page, listed as live.
        (await ListAsync(staying)).Should().HaveCount(1);
    }

    [Fact]
    public async Task Changing_a_password_ends_every_session()
    {
        // ADR-028 seen through the new vocabulary.
        var email = await NewAccountAsync();

        await SignInAsync(email);
        var mine = await SignInAsync(email);

        (await ListAsync(mine)).Should().HaveCount(2);

        var changed = await mine.SendJsonAsync(
            _client,
            HttpMethod.Post,
            "/api/auth/change-password",
            new { currentPassword = Password, newPassword = "a replacement password" });

        changed.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var (after, _) = await Session.LoginAsync(
            _client, email, "a replacement password");

        (await ListAsync(after)).Should().HaveCount(1);
    }
}
