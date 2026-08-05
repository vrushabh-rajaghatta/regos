using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using RegOS.Persistence;
using RegOS.Persistence.Initialization;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.TestSupport;

/// <summary>
/// A database of this assembly's own, created from the current migration chain
/// and seeded by the initializers the API boots with.
/// </summary>
/// <remarks>
/// <para>
/// <b>Green means "nothing collided", not "the schema is current"</b> — the
/// finding this exists to close
/// (<see href="../../../docs/adr/ADR-064-the-test-suite-provisions-its-own-schema.md">ADR-064</see>).
/// A stale schema only turns a test red when a migration happens to touch a read
/// path some existing test already exercises, so a suite pointed at a database
/// nothing migrates can be five migrations behind and still pass.
/// </para>
/// <para>
/// <b>Per assembly, not per run</b> (ADR-064 §2). <c>dotnet test</c> gives every
/// test project its own process and nothing coordinates them, so "a database per
/// run" has no implementation. Tests within an assembly still share this one,
/// which is why each of them still cleans up what it creates — per-assembly
/// provisioning replaces <em>cross-run</em> isolation, not <em>intra-assembly</em>
/// isolation.
/// </para>
/// <para>
/// <b>Abstract, and each assembly declares a one-line subclass.</b> That is not
/// ceremony: <c>GetType().Assembly</c> then names the <em>test</em> assembly
/// rather than this one, which is what the database is named after and what a
/// developer reads in <c>\l</c> when a run dies half-way and leaves one behind.
/// </para>
/// </remarks>
public abstract class RegOSTestDatabase : IAsyncLifetime
{
    private ServiceProvider? _services;
    private bool _disposed;

    protected RegOSTestDatabase()
    {
        Name = DatabaseName(GetType().Assembly.GetName().Name);

        ConnectionString = new NpgsqlConnectionStringBuilder(TestPostgres.Server)
        {
            Database = Name
        }.ConnectionString;

        Options = new DbContextOptionsBuilder<RegOSDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
    }

    /// <summary>The database this assembly provisioned, for failure messages.</summary>
    public string Name { get; }

    public string ConnectionString { get; }

    public DbContextOptions<RegOSDbContext> Options { get; }

    /// <summary>
    /// A context scoped to <paramref name="tenant"/>. Passing none gives the
    /// unauthenticated view the global query filters see before a login
    /// (ADR-031) — which shows no tenant-owned rows at all, and is almost never
    /// what a test wants.
    /// </summary>
    public RegOSDbContext NewContext(ITenantContext? tenant = null) =>
        new(Options, tenant ?? NoTenant.Instance);

    public async Task InitializeAsync()
    {
        await OnServerAsync($"""CREATE DATABASE "{Name}";""");

        var services = new ServiceCollection();

        // The same registration the API makes, by the same method, so an
        // initializer added tomorrow runs here without anyone remembering to
        // wire it up (ADR-064 §4).
        services.AddPersistence(ConnectionString);
        services.AddScoped<ITenantContext, NoTenant>();

        _services = services.BuildServiceProvider();

        await using var scope = _services.CreateAsyncScope();

        await scope.ServiceProvider
            .GetRequiredService<RegOSDbContext>()
            .Database.MigrateAsync();

        foreach (var initializer in scope.ServiceProvider.GetServices<IDataInitializer>())
        {
            await initializer.InitializeAsync();
        }
    }

    /// <summary>
    /// <b>Idempotent, and that is not defensive coding.</b> A fixture reached
    /// through more than one disposal path — xUnit's <c>IAsyncLifetime</c> and
    /// <c>IAsyncDisposable</c> are both candidates, and
    /// <c>WebApplicationFactory</c> brings its own — would otherwise drop the
    /// database twice and race itself.
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        if (_services is not null)
            await _services.DisposeAsync();

        // Npgsql pools connections per connection string and keeps them open
        // after the last context is disposed. DROP blocks on those, and the
        // FORCE below would terminate them — but clearing our own pool first
        // means the only thing FORCE ever has to kill is somebody's psql
        // session, which is a much smaller thing to be doing by surprise.
        NpgsqlConnection.ClearAllPools();

        await OnServerAsync($"""DROP DATABASE IF EXISTS "{Name}" WITH (FORCE);""");
    }

    private static async Task OnServerAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(TestPostgres.Server);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// <c>regos_test_&lt;assembly&gt;_&lt;short guid&gt;</c>, and the truncation is
    /// load-bearing: a Postgres identifier is capped at 63 characters and is
    /// <em>silently</em> truncated past it, so two assemblies with a long shared
    /// prefix would otherwise collide on a name neither of them chose.
    /// </summary>
    private static string DatabaseName(string? assemblyName)
    {
        var slug = (assemblyName ?? "unknown")
            .Replace("RegOS.", string.Empty, StringComparison.Ordinal)
            .Replace(".Tests", string.Empty, StringComparison.Ordinal)
            .Replace('.', '_')
            .ToLowerInvariant();

        if (slug.Length > 28)
            slug = slug[..28];

        return $"regos_test_{slug}_{Guid.NewGuid():N}"[..(11 + slug.Length + 1 + 8)];
    }

    /// <summary>
    /// What the API's tenant context resolves to at boot: no identity, so the
    /// filters show nothing, and asking for a tenant outright is an error. The
    /// initializers are written against exactly this — several call
    /// <c>IgnoreQueryFilters()</c> because of it.
    /// </summary>
    private sealed class NoTenant : ITenantContext
    {
        public static readonly NoTenant Instance = new();

        public TenantId TenantId =>
            throw new InvalidOperationException(
                "No tenant. A test that needs tenant-scoped rows passes its own "
                + "ITenantContext to NewContext(); this is the seeding view.");

        public TenantId? TenantIdOrNull => null;
    }
}
