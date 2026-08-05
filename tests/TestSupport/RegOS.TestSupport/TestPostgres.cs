namespace RegOS.TestSupport;

/// <summary>
/// The one place a test knows where Postgres is.
/// </summary>
/// <remarks>
/// <para>
/// <b>One place, not twenty-seven.</b> Before
/// <see href="../../../docs/adr/ADR-064-the-test-suite-provisions-its-own-schema.md">ADR-064</see>,
/// twenty-seven test files each carried a literal connection string naming the
/// developer's own working database. Pointing the suite anywhere else meant
/// editing all of them, which is the same as saying it could not be done.
/// </para>
/// <para>
/// <b>This names a server, never a database.</b> The database is created per
/// test assembly by <see cref="RegOSTestDatabase"/>; what lives here is only how
/// to reach the server that will hold it, which is why the connection string
/// below points at the <c>postgres</c> maintenance database — the one connection
/// that must exist before any RegOS database does.
/// </para>
/// </remarks>
public static class TestPostgres
{
    /// <summary>
    /// Environment variable holding a connection string to the <b>server</b>,
    /// for a machine whose Postgres is not the developer default — a CI runner,
    /// or a second local instance.
    /// </summary>
    public const string ServerVariable = "REGOS_TEST_POSTGRES";

    /// <summary>
    /// The local development Postgres, which is what every RegOS developer runs
    /// today. A default rather than a required variable, because a test suite
    /// that will not run until you export something is a test suite people stop
    /// running.
    /// </summary>
    private const string LocalDefault =
        "Host=localhost;Port=5432;Database=postgres;Username=admin;Password=password123";

    public static string Server =>
        Environment.GetEnvironmentVariable(ServerVariable) is { Length: > 0 } configured
            ? configured
            : LocalDefault;
}
