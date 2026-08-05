using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using RegOS.Platform.Application.Services;

namespace RegOS.Api.Tests;

/// <summary>
/// Hosts the real API in-process.
/// </summary>
/// <remarks>
/// <para>
/// Runs in <c>Development</c> deliberately. That loads the same configuration a
/// developer runs with and seeds <c>dev@regos.local</c>, which is the only
/// account with a password until invitation acceptance exists.
/// </para>
/// <para>
/// It talks to real Postgres — <b>this assembly's own database</b> since
/// ADR-064, rather than the developer's. These tests are still not hermetic and
/// still need a server up; what changed is that the schema they run against is
/// produced by the migration chain rather than assumed to be current. The
/// alternative was an in-memory provider, and an authentication test that never
/// touches the real store proves very little about the real store.
/// </para>
/// </remarks>
public sealed class RegOSApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    /// <summary>
    /// <b>Owned here rather than declared as a second collection fixture</b>,
    /// because xUnit 2 does not inject one fixture into another — a fixture
    /// constructor takes only <c>IMessageSink</c>, and asking for anything else
    /// fails every test in the collection with *"unresolved constructor
    /// arguments"* before a line of it runs.
    /// </summary>
    private readonly ApiDatabase _database = new();

    Task IAsyncLifetime.InitializeAsync() => _database.InitializeAsync();

    // Both, deliberately: xUnit may reach a fixture through IAsyncLifetime or
    // through IAsyncDisposable, WebApplicationFactory supplies the latter, and
    // the database's own disposal is idempotent so either route is safe.
    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }

    /// <summary>
    /// Direct database access for what HTTP cannot express, pointed at the same
    /// database the host is. Exposed here rather than left static, because the
    /// connection string is now decided at run time.
    /// </summary>
    public UserStore Users => new(_database.ConnectionString);

    public RefreshTokenStore RefreshTokens => new(_database.ConnectionString);

    /// <summary>
    /// Captures the invitation tokens the API would have emailed. Shared across
    /// the fixture, because the factory is shared.
    /// </summary>
    public CapturingInvitationNotifier Invitations { get; } = new();

    /// <summary>
    /// Captures the reset links the API would have emailed. Shared across the
    /// fixture, because the factory is shared.
    /// </summary>
    public CapturingPasswordResetNotifier PasswordResets { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // The one substitution that is not a test double: the host reads its
        // connection string from configuration, and configuration names the
        // developer's database. Overriding it here is what puts the real
        // Program — its initializers included — on this assembly's schema.
        builder.UseSetting("ConnectionStrings:RegOS", _database.ConnectionString);

        // The only substitutions these tests make. Everything else - the real
        // authentication handler, the real middleware order, the real database
        // - is exercised as it ships. Acceptance and reset tokens exist only in
        // transit, so without these there is no way to test redeeming one.
        builder.ConfigureTestServices(services =>
        {
            services.AddScoped<IInvitationNotifier>(_ => Invitations);
            services.AddScoped<IPasswordResetNotifier>(_ => PasswordResets);
        });
    }

    /// <summary>
    /// A client that does <em>not</em> follow redirects or manage cookies. The
    /// tests carry cookies themselves (see <see cref="Session"/>) so that what
    /// is asserted is the actual <c>Set-Cookie</c> header, not a cookie jar's
    /// interpretation of it.
    /// </summary>
    public HttpClient NewClient() =>
        CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = false
        });
}
