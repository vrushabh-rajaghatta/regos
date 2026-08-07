using System.Net;
using System.Text.Json;

using FluentAssertions;

namespace RegOS.Api.Tests;

/// <summary>
/// The playbook read routes, through the real pipeline — routing, the tenant
/// resolved from a real session, the query filter, serialisation.
/// </summary>
/// <remarks>
/// The database-backed tests in <c>RegOS.Process.Application.Tests</c> prove the
/// queries; this proves the two things they cannot. **That the routes exist and
/// are reachable**, and — the one worth having — **that the tenant filter is
/// applied to a real request**: playbooks take ADR-031's shared-plus-extensible
/// shape, and a handler that read them with no tenant would return the platform's
/// rows to an anonymous caller.
/// </remarks>
[Collection(ApiCollection.Name)]
public sealed class PlaybookEndpointTests : IAsyncLifetime
{
    private readonly HttpClient _client;

    private Session _session = default!;

    public PlaybookEndpointTests(RegOSApiFactory factory)
    {
        _client = factory.NewClient();
    }

    public async Task InitializeAsync()
        => (_session, _) = await Session.LoginAsync(_client);

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task An_unauthenticated_caller_sees_no_playbooks()
    {
        var response = await _client.GetAsync("/api/process-definitions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task The_seeded_playbook_is_served()
    {
        var response = await _session.SendAsync(
            _client, HttpMethod.Get, "/api/process-definitions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var body = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        var playbook = body.RootElement.EnumerateArray()
            .Should().ContainSingle(x =>
                x.GetProperty("code").GetString() == "US-FDA-IND-INITIAL")
            .Subject;

        playbook.GetProperty("isShared").GetBoolean().Should().BeTrue();
        playbook.GetProperty("currentVersionNumber").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task One_playbook_is_served_whole()
    {
        var list = await _session.SendAsync(
            _client, HttpMethod.Get, "/api/process-definitions");

        using var listBody = JsonDocument.Parse(
            await list.Content.ReadAsStringAsync());

        var id = listBody.RootElement.EnumerateArray()
            .First(x => x.GetProperty("code").GetString() == "US-FDA-IND-INITIAL")
            .GetProperty("id").GetString();

        var response = await _session.SendAsync(
            _client, HttpMethod.Get, $"/api/process-definitions/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var body = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        body.RootElement.GetProperty("steps").GetArrayLength().Should().Be(12);
        body.RootElement.GetProperty("selectedVersionNumber").GetInt32()
            .Should().Be(1);
    }

    /// <summary>
    /// The middleware maps <c>NotFoundException</c> to 404 — the endpoint carries
    /// no null check and no catch, like every other capability (ADR-012).
    /// </summary>
    [Fact]
    public async Task An_unknown_playbook_is_a_404()
    {
        var response = await _session.SendAsync(
            _client,
            HttpMethod.Get,
            $"/api/process-definitions/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
