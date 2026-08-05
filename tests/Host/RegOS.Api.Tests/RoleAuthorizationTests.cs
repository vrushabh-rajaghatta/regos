using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

namespace RegOS.Api.Tests;

/// <summary>
/// The role gate on user administration (ADR-033), proven through the real
/// pipeline: a Member is refused with 403 — not 404, because the route is no
/// secret; not 401, because we know exactly who they are — while the tenant
/// administrator who invited them keeps working.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class RoleAuthorizationTests : IAsyncLifetime
{
    private const string Marker = "roletest";

    private readonly RegOSApiFactory _factory;
    private readonly HttpClient _client;

    private Session _admin = default!;
    private Session _member = default!;

    public RoleAuthorizationTests(RegOSApiFactory factory)
    {
        _factory = factory;
        _client = factory.NewClient();
    }

    public async Task InitializeAsync()
    {
        (_admin, _) = await Session.LoginAsync(_client);

        // A real member, made the only way one can be made: invited by the
        // tenant administrator and accepting with a password of their own.
        var email = $"{Marker}.{Guid.NewGuid():N}@policy.example";

        var invited = await _admin.SendJsonAsync(
            _client,
            HttpMethod.Post,
            "/api/platform/users/invitations",
            new { firstName = "Ordinary", lastName = "Member", email });

        invited.StatusCode.Should().Be(HttpStatusCode.Created);

        const string password = "a good enough password";

        var accepted = await _client.PostAsJsonAsync(
            "/api/auth/invitations/accept",
            new { token = _factory.Invitations.TokenFor(email), password });

        accepted.EnsureSuccessStatusCode();

        (_member, _) = await Session.LoginAsync(_client, email, password);
    }

    public async Task DisposeAsync()
    {
        await _factory.RefreshTokens.DeleteAllForAsync(Session.DevEmail);
        await _factory.Users.DeleteUsersMatchingAsync(Marker);
    }

    [Fact]
    public async Task A_member_cannot_invite_users()
    {
        var response = await _member.SendJsonAsync(
            _client,
            HttpMethod.Post,
            "/api/platform/users/invitations",
            new
            {
                firstName = "Should",
                lastName = "NotExist",
                email = $"{Marker}.never.{Guid.NewGuid():N}@policy.example"
            });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task A_member_cannot_read_the_user_directory()
    {
        var response = await _member.SendAsync(
            _client, HttpMethod.Get, "/api/platform/users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task A_member_still_reaches_their_own_session_and_settings()
    {
        // The gate is on administration, not on being a user: /me and the
        // session surface must keep working for everyone.
        var me = await _member.SendAsync(
            _client, HttpMethod.Get, "/api/auth/me");

        me.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await me.Content
            .ReadFromJsonAsync<Dictionary<string, object>>();

        body!["role"].ToString().Should().Be("Member");
    }

    [Fact]
    public async Task The_tenant_administrator_still_administers()
    {
        var response = await _admin.SendAsync(
            _client, HttpMethod.Get, "/api/platform/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
