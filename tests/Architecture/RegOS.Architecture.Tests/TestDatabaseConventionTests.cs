using System.Text.RegularExpressions;

using FluentAssertions;

namespace RegOS.Architecture.Tests;

/// <summary>
/// <b>Exactly one file under <c>tests/</c> knows where a database is.</b>
/// </summary>
/// <remarks>
/// <para>
/// Before <see href="../../../docs/adr/ADR-064-the-test-suite-provisions-its-own-schema.md">ADR-064</see>,
/// twenty-seven test files each carried a literal connection string naming the
/// developer's working database. Nothing migrated it, so the suite silently
/// assumed somebody already had — and a stale schema only turns a test red when
/// a migration happens to touch a read path some test already exercises. The
/// database was five migrations behind on the day 18 of 19 suites went red, and
/// one migration behind on a day everything passed.
/// </para>
/// <para>
/// <b>The guard is "one file", not "not that one database".</b> A rule phrased
/// against <c>Database=regos</c> is satisfied by writing
/// <c>Database=regos_scratch</c>, which reintroduces the defect under a new
/// name. What matters is that the decision lives in one place.
/// </para>
/// </remarks>
public class TestDatabaseConventionTests
{
    /// <summary>
    /// The one file allowed to name a server, and the reason it is allowed: it
    /// names a <em>server</em> and a maintenance database, never a RegOS one.
    /// The database each assembly runs against is created at run time.
    /// </summary>
    private const string TheOnePlace =
        "tests/TestSupport/RegOS.TestSupport/TestPostgres.cs";

    private static readonly Regex ConnectionString = new(
        @"Host\s*=\s*[^""';]+;.*?Database\s*=", RegexOptions.Compiled);

    [Fact]
    public void Only_one_test_file_carries_a_connection_string()
    {
        var offenders = Repo.SourceFiles("tests")
            .Where(path => Repo.Relative(path) != TheOnePlace)
            .Where(path => ConnectionString.IsMatch(Repo.CodeOf(path)))
            .Select(Repo.Relative)
            .ToList();

        offenders.Should().BeEmpty(
            "a test that names its own database is a test running against a "
            + "schema nothing migrated; take RegOSTestDatabase instead, and if "
            + "the server itself has moved, change TestPostgres");
    }

    /// <summary>
    /// The negative control, in the direction that matters here: the one place
    /// must still be a real file. A rename would otherwise leave the assertion
    /// above passing over a repository with no connection string anywhere and
    /// no way to reach a database.
    /// </summary>
    [Fact]
    public void The_one_place_still_exists_and_still_carries_one()
    {
        var path = Path.Combine(Repo.Root, TheOnePlace);

        File.Exists(path).Should().BeTrue(
            $"{TheOnePlace} is the single place a test knows where Postgres is");

        ConnectionString.IsMatch(File.ReadAllText(path)).Should().BeTrue(
            "if this stopped matching, the test above would be scanning for a "
            + "pattern that no longer describes a connection string");
    }
}
