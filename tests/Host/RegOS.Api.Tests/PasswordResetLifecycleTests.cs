using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using RegOS.Api.Authentication;

namespace RegOS.Api.Tests;

/// <summary>
/// Request → reset → sign in with the new password, and every way that must not
/// work.
///
/// Each test owns the account it resets: it invites a user, accepts on their
/// behalf, and deletes them afterwards (ADR-019 principle 7). Resetting the
/// shared development account instead would change its password for every other
/// test in the run.
///
/// This layer also reaches two things no other can. Requesting a reset always
/// answers 204, so only the captured notifier can say whether a link was
/// actually sent — and only here can a deliberately expired or replayed link be
/// presented.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class PasswordResetLifecycleTests : IAsyncLifetime
{
    private const string Marker = "resettest";
    private const string OriginalPassword = "the original password";
    private const string NewPassword = "a brand new password";

    private readonly RegOSApiFactory _factory;
    private readonly HttpClient _client;

    private Session _admin = default!;

    public PasswordResetLifecycleTests(RegOSApiFactory factory)
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
        await RefreshTokenStore.DeleteAllForAsync(Session.DevEmail);
        await UserStore.DeleteUsersMatchingAsync(Marker);
    }

    private static string NewEmail() =>
        $"{Marker}.{Guid.NewGuid():N}@policy.example";

    /// <summary>Creates an active account this test owns, with a known password.</summary>
    private async Task<(string Email, Guid UserId)> NewActiveUserAsync()
    {
        var email = NewEmail();

        var invited = await _admin.SendJsonAsync(
            _client,
            HttpMethod.Post,
            "/api/platform/users/invitations",
            new { firstName = "Reset", lastName = "Person", email });

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

        return (email, userId);
    }

    private Task<HttpResponseMessage> RequestAsync(string? email) =>
        _client.PostAsJsonAsync(
            "/api/auth/password-reset/request", new { email });

    private Task<HttpResponseMessage> CompleteAsync(
        string? token, string password = NewPassword) =>
        _client.PostAsJsonAsync(
            "/api/auth/password-reset/complete", new { token, password });

    /// <summary>Requests a reset and returns the link that was sent.</summary>
    private async Task<string> RequestAndCaptureAsync(string email)
    {
        (await RequestAsync(email)).StatusCode
            .Should().Be(HttpStatusCode.NoContent);

        return _factory.PasswordResets.TokenFor(email);
    }

    // -------------------------------------------------------------- requesting

    [Fact]
    public async Task Requesting_a_reset_sends_a_link()
    {
        var (email, _) = await NewActiveUserAsync();

        var token = await RequestAndCaptureAsync(email);

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task A_reset_is_never_stored_in_plaintext()
    {
        var (email, userId) = await NewActiveUserAsync();

        var token = await RequestAndCaptureAsync(email);

        var stored = await UserStore.PasswordResetHashesForAsync(userId);

        stored.Should().NotBeEmpty();
        stored.Should().NotContain(token);
    }

    [Fact]
    public async Task Requesting_again_invalidates_the_previous_link()
    {
        // At most one live link per user, or an old email keeps working
        // alongside the new one.
        var (email, _) = await NewActiveUserAsync();

        var first = await RequestAndCaptureAsync(email);
        var second = await RequestAndCaptureAsync(email);

        second.Should().NotBe(first);

        (await CompleteAsync(first)).StatusCode
            .Should().Be(HttpStatusCode.Unauthorized);

        (await CompleteAsync(second)).StatusCode
            .Should().Be(HttpStatusCode.NoContent);
    }

    // ----------------------------------------------------------- the silence

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-an-address")]
    [InlineData("nobody.at.all@policy.example")]
    public async Task Requesting_for_an_address_that_cannot_receive_one_still_answers_204(
        string? email)
    {
        // The whole security property of this endpoint: from outside, "sent"
        // and "silently ignored" are the same response, so the endpoint cannot
        // be used to discover which addresses have accounts (ADR-022).
        var response = await RequestAsync(email);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task No_link_is_sent_to_a_user_who_has_not_accepted_their_invitation()
    {
        // Reset recovers a credential; it does not create one. Invitation stays
        // the only route to a first password (ADR-027).
        var email = NewEmail();

        var invited = await _admin.SendJsonAsync(
            _client,
            HttpMethod.Post,
            "/api/platform/users/invitations",
            new { firstName = "Never", lastName = "Accepted", email });

        invited.StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await RequestAsync(email);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _factory.PasswordResets.Sent(email).Should().BeFalse();
    }

    [Fact]
    public async Task No_link_is_sent_to_a_deactivated_user()
    {
        var (email, userId) = await NewActiveUserAsync();

        var deactivate = await _admin.SendAsync(
            _client,
            HttpMethod.Post,
            $"/api/platform/users/{userId}/deactivate");

        deactivate.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var response = await RequestAsync(email);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _factory.PasswordResets.Sent(email).Should().BeFalse();
    }

    // -------------------------------------------------------------- completing

    [Fact]
    public async Task Completing_replaces_the_password()
    {
        var (email, _) = await NewActiveUserAsync();

        var token = await RequestAndCaptureAsync(email);

        (await CompleteAsync(token)).StatusCode
            .Should().Be(HttpStatusCode.NoContent);

        var (_, withNew) = await Session.LoginAsync(_client, email, NewPassword);

        withNew.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var (_, withOld) = await Session.LoginAsync(
            _client, email, OriginalPassword);

        withOld.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Completing_issues_no_session()
    {
        // Holding the link proves control of a mailbox, not knowledge of the
        // password just chosen. The user signs in afterwards like anyone else.
        var (email, _) = await NewActiveUserAsync();

        var token = await RequestAndCaptureAsync(email);

        var response = await CompleteAsync(token);

        response.Headers.Contains("Set-Cookie").Should().BeFalse();
    }

    [Fact]
    public async Task Completing_ends_every_session_opened_with_the_old_password()
    {
        // Whoever forced the reset, the sessions that predate it can no longer
        // be assumed to belong to the rightful owner.
        var (email, _) = await NewActiveUserAsync();

        var (session, _) = await Session.LoginAsync(
            _client, email, OriginalPassword);

        session.Refresh.Should().NotBeNull();

        var token = await RequestAndCaptureAsync(email);

        (await CompleteAsync(token)).StatusCode
            .Should().Be(HttpStatusCode.NoContent);

        var refresh = await Session.SendWithCookieAsync(
            _client,
            HttpMethod.Post,
            "/api/auth/refresh",
            SessionCookies.RefreshToken,
            session.Refresh!);

        refresh.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task A_reset_cannot_be_used_twice()
    {
        var (email, _) = await NewActiveUserAsync();

        var token = await RequestAndCaptureAsync(email);

        await CompleteAsync(token);

        var replay = await CompleteAsync(token, "yet another password");

        replay.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // And the replayed password was not the one that took effect.
        var (_, login) = await Session.LoginAsync(_client, email, NewPassword);

        login.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task An_unknown_token_is_rejected()
    {
        var response = await CompleteAsync("a-token-that-was-never-issued");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task A_missing_token_is_rejected(string? token)
    {
        var response = await CompleteAsync(token);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task An_expired_reset_is_rejected()
    {
        var (email, userId) = await NewActiveUserAsync();

        var token = await RequestAndCaptureAsync(email);

        await UserStore.ExpirePasswordResetsForAsync(userId);

        (await CompleteAsync(token)).StatusCode
            .Should().Be(HttpStatusCode.Unauthorized);

        // And the old password still works, so nothing was half-applied.
        var (_, login) = await Session.LoginAsync(
            _client, email, OriginalPassword);

        login.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task A_reset_for_a_user_deactivated_since_is_rejected()
    {
        // A mailbox must not restore access somebody withdrew deliberately.
        var (email, userId) = await NewActiveUserAsync();

        var token = await RequestAndCaptureAsync(email);

        await _admin.SendAsync(
            _client,
            HttpMethod.Post,
            $"/api/platform/users/{userId}/deactivate");

        (await CompleteAsync(token)).StatusCode
            .Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task A_password_that_fails_the_rules_leaves_the_link_usable()
    {
        // A typo must be recoverable: the reset is only spent once a password
        // has actually been accepted.
        var (email, _) = await NewActiveUserAsync();

        var token = await RequestAndCaptureAsync(email);

        var response = await CompleteAsync(token, "short");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await CompleteAsync(token)).StatusCode
            .Should().Be(HttpStatusCode.NoContent);
    }
}
