using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

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
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
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
