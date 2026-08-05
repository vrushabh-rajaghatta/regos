using System.Net;
using System.Net.Http.Json;

using FluentAssertions;
using RegOS.Platform.Contracts;

namespace RegOS.Api.Tests;

/// <summary>
/// Invite → accept → sign in, and every way that must not work.
///
/// Browser automation is deliberately absent: a user cannot be deleted, so an
/// invite spec would leak a row per run (ADR-019 rule 1). This layer can create
/// and clean up its own users, and reaches things a browser could not — a
/// deliberately expired token, a replayed one, a resend that must invalidate
/// its predecessor.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class InvitationLifecycleTests : IAsyncLifetime
{
    private const string Marker = "invitationtest";

    private readonly RegOSApiFactory _factory;
    private readonly HttpClient _client;

    private Session _admin = default!;

    public InvitationLifecycleTests(RegOSApiFactory factory)
    {
        _factory = factory;
        _client = factory.NewClient();
    }

    public async Task InitializeAsync()
    {
        (_admin, _) = await Session.LoginAsync(_client);
    }

    /// <summary>
    /// Every user this class invites, and — by cascade (ADR-026) — their
    /// invitations and credentials with them. Principle 7.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _factory.RefreshTokens.DeleteAllForAsync(Session.DevEmail);
        await _factory.Users.DeleteUsersMatchingAsync(Marker);
    }

    private static string NewEmail() =>
        $"{Marker}.{Guid.NewGuid():N}@policy.example";

    /// <summary>Invites someone and returns their email and acceptance token.</summary>
    private async Task<(string Email, Guid UserId, string Token)> InviteAsync()
    {
        var email = NewEmail();

        var response = await _admin.SendJsonAsync(
            _client,
            HttpMethod.Post,
            "/api/platform/users/invitations",
            new { firstName = "Invited", lastName = "Person", email });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content
            .ReadFromJsonAsync<Dictionary<string, object>>();

        var userId = Guid.Parse(created!["id"].ToString()!);

        return (email, userId, _factory.Invitations.TokenFor(email));
    }

    private Task<HttpResponseMessage> AcceptAsync(
        string? token, string password = "a good enough password") =>
        _client.PostAsJsonAsync(
            "/api/auth/invitations/accept", new { token, password });

    // --------------------------------------------------------------- inviting

    [Fact]
    public async Task Inviting_a_user_sends_them_an_acceptance_token()
    {
        var (email, _, token) = await InviteAsync();

        _factory.Invitations.Sent(email).Should().BeTrue();
        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task An_invitation_is_never_stored_in_plaintext()
    {
        var (_, userId, token) = await InviteAsync();

        var stored = await _factory.Users.InvitationHashesForAsync(userId);

        stored.Should().NotBeEmpty();
        stored.Should().NotContain(token);
    }

    // -------------------------------------------------------------- accepting

    [Fact]
    public async Task Accepting_activates_the_user_and_sets_their_password()
    {
        var (email, userId, token) = await InviteAsync();

        var response = await AcceptAsync(token);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await _factory.Users.StatusOfAsync(userId)).Should().Be(0);   // Active
        (await _factory.Users.CredentialCountAsync(userId)).Should().Be(1);

        // The invariant this slice exists to establish, checked end to end:
        // an active user has exactly one credential, and it works.
        var (_, login) = await Session.LoginAsync(
            _client, email, "a good enough password");

        login.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task An_invitation_cannot_be_accepted_twice()
    {
        var (_, _, token) = await InviteAsync();

        await AcceptAsync(token);

        var replay = await AcceptAsync(token, "another password entirely");

        replay.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task An_unknown_token_is_rejected()
    {
        var response = await AcceptAsync("a-token-that-was-never-issued");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task A_missing_token_is_rejected(string? token)
    {
        var response = await AcceptAsync(token);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task An_expired_invitation_is_rejected()
    {
        var (_, userId, token) = await InviteAsync();

        await _factory.Users.ExpireInvitationsForAsync(userId);

        var response = await AcceptAsync(token);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await _factory.Users.StatusOfAsync(userId)).Should().Be(2);   // still Invited
    }

    [Fact]
    public async Task An_invitation_to_a_deactivated_user_is_rejected()
    {
        // Someone withdrew access after inviting them. The invitation no longer
        // represents the organization's intent (ADR-027).
        var (_, userId, token) = await InviteAsync();

        var deactivate = await _admin.SendAsync(
            _client,
            HttpMethod.Post,
            $"/api/platform/users/{userId}/deactivate");

        deactivate.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var response = await AcceptAsync(token);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await _factory.Users.CredentialCountAsync(userId)).Should().Be(0);
    }

    [Fact]
    public async Task A_password_that_fails_the_rules_leaves_the_user_invited()
    {
        // The ordering that matters: if anything fails, the user must not be
        // left Active without a credential.
        var (_, userId, token) = await InviteAsync();

        var response = await AcceptAsync(token, "short");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await _factory.Users.StatusOfAsync(userId)).Should().Be(2);   // Invited
        (await _factory.Users.CredentialCountAsync(userId)).Should().Be(0);

        // And the invitation still works afterwards, so a typo is recoverable.
        (await AcceptAsync(token)).StatusCode
            .Should().Be(HttpStatusCode.NoContent);
    }

    // -------------------------------------------------------------- resending

    [Fact]
    public async Task Resending_invalidates_the_previous_token()
    {
        var (email, userId, first) = await InviteAsync();

        var resend = await _admin.SendAsync(
            _client,
            HttpMethod.Post,
            $"/api/platform/users/{userId}/invitations");

        resend.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var second = _factory.Invitations.TokenFor(email);

        second.Should().NotBe(first);

        // At most one live token per user: the old link must stop working the
        // moment a new one is sent.
        (await AcceptAsync(first)).StatusCode
            .Should().Be(HttpStatusCode.Unauthorized);

        (await AcceptAsync(second)).StatusCode
            .Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task A_user_who_has_already_accepted_cannot_be_re_invited()
    {
        var (_, userId, token) = await InviteAsync();

        await AcceptAsync(token);

        var resend = await _admin.SendAsync(
            _client,
            HttpMethod.Post,
            $"/api/platform/users/{userId}/invitations");

        resend.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // ------------------------------------------------- the closed-off shortcut

    [Fact]
    public async Task An_administrator_cannot_activate_an_invited_user()
    {
        // The path that used to produce an Active user with no credential.
        // Closing it is what makes the invariant enforceable (ADR-027).
        var (_, userId, _) = await InviteAsync();

        var response = await _admin.SendAsync(
            _client,
            HttpMethod.Post,
            $"/api/platform/users/{userId}/activate");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await _factory.Users.StatusOfAsync(userId)).Should().Be(2);
        (await _factory.Users.CredentialCountAsync(userId)).Should().Be(0);
    }

    [Fact]
    public async Task An_administrator_can_still_reinstate_a_deactivated_user()
    {
        // The remaining meaning of Activate, and it must keep working.
        var (_, userId, token) = await InviteAsync();

        await AcceptAsync(token);

        await _admin.SendAsync(
            _client,
            HttpMethod.Post,
            $"/api/platform/users/{userId}/deactivate");

        var response = await _admin.SendAsync(
            _client,
            HttpMethod.Post,
            $"/api/platform/users/{userId}/activate");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await _factory.Users.StatusOfAsync(userId)).Should().Be(0);
    }
}
