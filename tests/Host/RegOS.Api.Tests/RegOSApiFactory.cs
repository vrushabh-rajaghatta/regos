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
/// It talks to the same Postgres as every other test project. That is a real
/// trade — these tests are not hermetic and need the database up — but the
/// alternative is an in-memory provider, and an authentication test that never
/// touches the real store proves very little about the real store.
/// </para>
/// </remarks>
public sealed class RegOSApiFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Captures the invitation tokens the API would have emailed. Shared across
    /// the fixture, because the factory is shared.
    /// </summary>
    public CapturingInvitationNotifier Invitations { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // The only substitution these tests make. Everything else - the real
        // authentication handler, the real middleware order, the real database
        // - is exercised as it ships. An acceptance token exists only in transit,
        // so without this there is no way to test accepting one.
        builder.ConfigureTestServices(services =>
        {
            services.AddScoped<IInvitationNotifier>(_ => Invitations);
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
