using FluentAssertions;

namespace RegOS.Architecture.Tests;

/// <summary>
/// SC-005 — one handler per file, in a file named after it.
///
/// The convention that makes a slice navigable: knowing the capability's name
/// tells you the filename, without opening anything. Its opposite is a file
/// named for its folder — ContactQueries.cs, SubmissionEndpoints.cs — that
/// accumulates handlers until nobody can see what the context does.
///
/// This is also the rule that keeps the other conventions checkable. Query
/// records, command names and endpoint shape are all folder-scoped, and a
/// bundle file hides its contents from every one of them.
/// </summary>
public sealed class SliceLayoutConventionTests
{
    /// <summary>
    /// Bundled files that predate the rule. Shrink, never grow.
    /// </summary>
    private static readonly HashSet<string> Grandfathered = [];

    [Fact]
    public void Each_handler_lives_in_a_file_named_after_it()
    {
        var offenders = new List<string>();

        foreach (var file in ApplicationSources())
        {
            var declared = Handlers.DeclaredIn(Repo.CodeOf(file)).ToList();

            if (declared.Count == 0) continue;

            var expected = Path.GetFileNameWithoutExtension(file);
            var relative = Repo.Relative(file);

            if (Grandfathered.Contains(relative)) continue;

            if (declared.Count > 1)
            {
                offenders.Add(
                    $"{relative} declares {declared.Count} handlers "
                    + $"({string.Join(", ", declared)})");
            }
            else if (declared[0] != expected)
            {
                offenders.Add($"{relative} declares {declared[0]}");
            }
        }

        offenders.Should().BeEmpty(
            "a handler goes in its own file, named after the class "
            + "(slice-conventions.md SC-005). Split the bundle: one folder per "
            + "capability, one file per type inside it");
    }

    [Fact]
    public void The_grandfathered_list_contains_no_stale_entries()
    {
        var stale = Grandfathered
            .Where(path => !File.Exists(Path.Combine(Repo.Root, path)))
            .ToList();

        stale.Should().BeEmpty(
            "these files no longer exist — delete them from "
            + "SliceLayoutConventionTests.Grandfathered");
    }

    private static IEnumerable<string> ApplicationSources() =>
        Repo.SourceFiles("src")
            .Where(path => Repo.Relative(path) is var relative
                && (relative.Contains("/Commands/", StringComparison.Ordinal)
                    || relative.Contains("/Queries/", StringComparison.Ordinal)));
}
