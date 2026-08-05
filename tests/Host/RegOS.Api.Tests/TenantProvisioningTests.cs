using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

namespace RegOS.Api.Tests;

/// <summary>
/// The whole provisioning story (ADR-030/033/060), through the real pipeline:
/// the platform administrator creates a tenant; the tenant arrives with an
/// invited administrator and an empty organization registry; the administrator
/// accepts, signs in, sees exactly their own world; and a retired tenant's
/// sessions die at the next refresh.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class TenantProvisioningTests : IAsyncLifetime
{
    private const string Marker = "tenantprovtest";

    private readonly RegOSApiFactory _factory;
    private readonly HttpClient _client;

    private Session _platform = default!;
    private readonly List<Guid> _tenantIds = [];

    public TenantProvisioningTests(RegOSApiFactory factory)
    {
        _factory = factory;
        _client = factory.NewClient();
    }

    public async Task InitializeAsync()
    {
        (_platform, var response) = await Session.LoginAsync(
            _client, "platform@regos.local", "platform-password");

        response.EnsureSuccessStatusCode();
    }

    public async Task DisposeAsync()
    {
        await _factory.Users.DeleteUsersMatchingAsync(Marker);

        foreach (var id in _tenantIds)
        {
            // Anything the tenant's own administrator recorded first (no
            // cascade), then the tenant. Raw SQL: the API offers no tenant
            // deletion, on purpose.
            await _factory.Users.ExecuteAsync(
                """DELETE FROM "Organizations" WHERE "TenantId" = @id""", id);
            await _factory.Users.ExecuteAsync(
                """DELETE FROM "Tenants" WHERE "Id" = @id""", id);
        }
    }

    private async Task<(Guid TenantId, string AdminEmail, string Token)>
        ProvisionAsync(string name)
    {
        var email = $"{Marker}.{Guid.NewGuid():N}@policy.example";

        var response = await _platform.SendJsonAsync(
            _client,
            HttpMethod.Post,
            "/api/platform/tenants",
            new
            {
                name,
                adminEmail = email,
                adminFirstName = "First",
                adminLastName = "Administrator"
            });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content
            .ReadFromJsonAsync<Dictionary<string, object>>();

        var tenantId = Guid.Parse(created!["tenantId"].ToString()!);
        _tenantIds.Add(tenantId);

        return (tenantId, email, _factory.Invitations.TokenFor(email));
    }

    private async Task<Session> AcceptAndLoginAsync(string email, string token)
    {
        const string password = "a perfectly fine password";

        var accepted = await _client.PostAsJsonAsync(
            "/api/auth/invitations/accept", new { token, password });

        accepted.EnsureSuccessStatusCode();

        var (session, login) = await Session.LoginAsync(
            _client, email, password);

        login.EnsureSuccessStatusCode();

        return session;
    }

    [Fact]
    public async Task Provisioning_creates_a_tenant_and_its_invited_admin()
    {
        var (tenantId, email, token) = await ProvisionAsync(
            $"Prov Tenant {Guid.NewGuid():N}");

        // The invitation exists and is the only path to a password (ADR-027).
        token.Should().NotBeNullOrEmpty();

        var admin = await AcceptAndLoginAsync(email, token);

        // The token says who they are: the new tenant's administrator.
        var me = await admin.SendAsync(_client, HttpMethod.Get, "/api/auth/me");
        var body = await me.Content
            .ReadFromJsonAsync<Dictionary<string, object>>();

        body!["tenantId"].ToString().Should().Be(tenantId.ToString());
        body["role"].ToString().Should().Be("TenantAdministrator");

        // Their registry starts empty (ADR-060). Provisioning asserts no
        // regulatory party on the customer's behalf — not even the customer
        // itself; the administrator records that, and nothing arrives here
        // carrying the tenant's guid.
        var organizations = await admin.SendAsync(
            _client, HttpMethod.Get, "/api/organizations");
        var registry = await organizations.Content
            .ReadFromJsonAsync<List<Dictionary<string, object>>>();

        registry.Should().BeEmpty();
    }

    [Fact]
    public async Task The_new_admin_sees_only_their_own_tenant()
    {
        var (_, email, token) = await ProvisionAsync(
            $"Lonely Tenant {Guid.NewGuid():N}");

        var admin = await AcceptAndLoginAsync(email, token);

        // Their user directory: themselves, and nobody from any other tenant.
        var users = await admin.SendAsync(
            _client, HttpMethod.Get, "/api/platform/users");
        var page = await users.Content
            .ReadFromJsonAsync<Dictionary<string, object>>();

        page!["totalCount"].ToString().Should().Be("1");

        // And the platform's own directory is not theirs to see.
        var tenants = await admin.SendAsync(
            _client, HttpMethod.Get, "/api/platform/tenants");

        tenants.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task The_platform_admin_reads_a_tenants_users_across_the_boundary()
    {
        var (tenantId, email, _) = await ProvisionAsync(
            $"Visible Tenant {Guid.NewGuid():N}");

        var users = await _platform.SendAsync(
            _client, HttpMethod.Get, $"/api/platform/tenants/{tenantId}/users");

        users.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await users.Content
            .ReadFromJsonAsync<List<Dictionary<string, object>>>();

        list.Should().ContainSingle();
        list![0]["email"].ToString().Should().Be(email);
        list[0]["role"].ToString().Should().Be("TenantAdministrator");
    }

    [Fact]
    public async Task Retiring_a_tenant_ends_its_sessions_at_the_next_refresh()
    {
        var (tenantId, email, token) = await ProvisionAsync(
            $"Doomed Tenant {Guid.NewGuid():N}");

        var admin = await AcceptAndLoginAsync(email, token);

        var deactivated = await _platform.SendAsync(
            _client, HttpMethod.Post,
            $"/api/platform/tenants/{tenantId}/deactivate");

        deactivated.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // The refresh token they hold buys nothing once the tenant is retired.
        var refreshed = await admin.SendAsync(
            _client, HttpMethod.Post, "/api/auth/refresh");
        refreshed.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Neither does the password they know.
        var (_, login) = await Session.LoginAsync(
            _client, email, "a perfectly fine password");
        login.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
