using System.Text.RegularExpressions;

using FluentAssertions;

namespace RegOS.Architecture.Tests;

/// <summary>
/// <b>A test project that can reach the database provisions one.</b>
/// </summary>
/// <remarks>
/// <para>
/// <see href="../../../docs/adr/ADR-064-the-test-suite-provisions-its-own-schema.md">ADR-064</see>
/// gave every database-touching assembly a database of its own, created from the
/// current migration chain and dropped afterwards.
/// <see cref="TestDatabaseConventionTests"/> stops a connection string coming
/// back — but that is a weaker rule than it looks: it proves <b>no file names a
/// database</b>, not <b>every database test uses the fixture</b>. A new assembly
/// could build a context from <c>TestPostgres.Server</c> directly and pass,
/// because that string lives in the one file the rule permits.
/// </para>
/// <para>
/// <b>Found by asking, at EPIC-023's close, what its own green tick asserted.</b>
/// The rule was first written down as <em>"any test project referencing
/// RegOS.Persistence"</em> — and <b>no test project references it directly</b>.
/// All seven reach it transitively through <c>*.Infrastructure</c>, so the rule
/// as stated matched nothing at all. <b>A rule stated from memory is a
/// hypothesis</b>, and that one was false on first contact with the graph.
/// </para>
/// </remarks>
public class TestProjectDependencyTests
{
    private const string Persistence = "RegOS.Persistence";
    private const string TestSupport = "RegOS.TestSupport";

    /// <summary>
    /// <b>Reach, not reference.</b> A test project sees <c>RegOSDbContext</c>
    /// through whatever it happens to reference, so the rule has to follow the
    /// graph rather than read one file.
    /// </summary>
    [Fact]
    public void A_test_project_that_can_reach_persistence_takes_a_test_database()
    {
        var offenders = TestProjects()
            .Where(project => Reaches(project, Persistence))
            .Where(project => !References(project).Contains(TestSupport))
            .ToList();

        offenders.Should().BeEmpty(
            "a suite that can open a RegOSDbContext can run against a schema "
            + $"nobody migrated; reference {TestSupport} and take a "
            + "RegOSTestDatabase (ADR-064)");
    }

    /// <summary>
    /// And the other way, which is the rule S001 learned to add: a reference
    /// nobody needs is a permission that outlives its reason, and it makes the
    /// rule above look satisfied by projects it never applied to.
    /// </summary>
    [Fact]
    public void No_test_project_takes_a_test_database_it_cannot_use()
    {
        var offenders = TestProjects()
            .Where(project => References(project).Contains(TestSupport))
            .Where(project => !Reaches(project, Persistence))
            .ToList();

        offenders.Should().BeEmpty(
            $"{TestSupport} provisions a database for tests that reach one — a "
            + "project that cannot is carrying a reference for no reason");
    }

    /// <summary>
    /// The negative control, and it has to check both sides: a scan that found
    /// no test projects, or none reaching persistence, would let both assertions
    /// above pass over an empty list.
    /// </summary>
    [Fact]
    public void The_test_projects_are_actually_being_read()
    {
        var projects = TestProjects();

        projects.Should().HaveCountGreaterThan(15,
            "RegOS had 19 test projects when this was written");

        projects.Count(project => Reaches(project, Persistence))
            .Should().BeGreaterThan(5,
                "seven of them touched the database when this was written; a "
                + "much smaller number means the graph walk stopped early");
    }

    // --- reading the graph ---------------------------------------------------
    //
    // Copied from ContextDependencyTests rather than shared, on ADR-018: this is
    // the second demonstrated need, and the third is the moment to extract a
    // project-graph reader rather than the second.

    private static readonly Regex Reference = new(
        @"ProjectReference\s+Include=""([^""]*\.csproj)""", RegexOptions.Compiled);

    private static readonly Dictionary<string, string[]> Graph = Build();

    private static Dictionary<string, string[]> Build()
    {
        var graph = new Dictionary<string, string[]>(StringComparer.Ordinal);

        foreach (var directory in new[] { "src", "tests" })
        {
            foreach (var file in Directory.EnumerateFiles(
                         Path.Combine(Repo.Root, directory), "*.csproj",
                         SearchOption.AllDirectories))
            {
                graph[Path.GetFileNameWithoutExtension(file)] =
                    Reference.Matches(File.ReadAllText(file))
                        .Select(match => Path.GetFileNameWithoutExtension(
                            match.Groups[1].Value.Replace('\\', '/')))
                        .ToArray();
            }
        }

        return graph;
    }

    private static List<string> TestProjects() =>
        Graph.Keys
            .Where(name => name.EndsWith(".Tests", StringComparison.Ordinal))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();

    private static string[] References(string project) =>
        Graph.TryGetValue(project, out var references) ? references : [];

    private static bool Reaches(string project, string target)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Stack<string>(References(project));

        while (pending.Count > 0)
        {
            var next = pending.Pop();

            if (!seen.Add(next)) continue;
            if (next == target) return true;

            foreach (var reference in References(next))
                pending.Push(reference);
        }

        return false;
    }
}
