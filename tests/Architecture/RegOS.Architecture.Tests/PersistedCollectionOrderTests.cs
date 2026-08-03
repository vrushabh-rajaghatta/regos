using System.Text.RegularExpressions;

using FluentAssertions;

namespace RegOS.Architecture.Tests;

/// <summary>
/// <b>An invariant may not depend on the order its own storage hands things
/// back in.</b>
///
/// Six aggregates enforced *"business time only moves forward"* by comparing
/// against <c>_history[^1]</c> — the last element of the loaded list. In memory
/// that is the entry the aggregate appended last, so every domain unit test
/// passed. Loaded through an EF <c>Include</c> with no <c>OrderBy</c>, it is
/// whichever row the database returned last, and the rule silently stopped
/// holding against a real Postgres.
/// </summary>
/// <remarks>
/// <b>Why this is an architecture test and not six unit tests.</b> The defect is
/// invisible to a unit test by construction: the failure needs a persistence
/// round-trip, and the round-trip that reproduces it is not deterministic. A
/// test that only fails when Postgres happens to reorder rows is not a
/// regression net. This one reads the source, so it fails the same way every
/// time and on code written by someone who never read this file.
/// <para>
/// Ordering the <c>Include</c>s would also have worked today, and was rejected:
/// it leaves the domain silently order-dependent and arms the trap for the next
/// repository. The rule is defended where it lives.
/// </para>
/// </remarks>
public class PersistedCollectionOrderTests
{
    /// <summary>
    /// Indexing from the end of a backing field — <c>_history[^1]</c>,
    /// <c>_marketStatusHistory[^1]</c>, <c>_entries[^0]</c>.
    /// </summary>
    /// <remarks>
    /// Scoped to fields, by the leading underscore. A local <c>StringBuilder</c>
    /// or a freshly built list is not loaded from anywhere and its order is the
    /// author's own.
    /// </remarks>
    private static readonly Regex IndexedFromTheEnd = new(
        @"(?<field>_[A-Za-z0-9_]+)\s*\[\s*\^", RegexOptions.Compiled);

    [Fact]
    public void No_aggregate_reads_the_last_element_of_a_persisted_collection()
    {
        var offenders = new List<string>();

        foreach (var file in DomainSourceFiles())
        {
            // Comments are stripped, so the explanation left at each fixed site
            // — "Max, not [^1]" — is not read as the thing it warns against.
            foreach (Match match in IndexedFromTheEnd.Matches(Repo.CodeOf(file)))
            {
                offenders.Add(
                    $"{Repo.Relative(file)}: {match.Groups["field"].Value}[^…]");
            }
        }

        offenders.Should().BeEmpty(
            "the last element of a collection loaded from the database is the "
            + "last row it returned, not the latest entry — compare against "
            + "Max(x => x.OccurredOn) instead, and see MedicinalProduct."
            + "ChangeMarketStatus for why");
    }

    /// <summary>
    /// The negative control. If this stopped finding files the assertion above
    /// would pass by reading nothing — the failure mode this repository has
    /// already been bitten by when counting test suites.
    /// </summary>
    [Fact]
    public void The_domain_projects_are_actually_being_read()
    {
        DomainSourceFiles().Should().HaveCountGreaterThan(100,
            "RegOS has nine bounded contexts; a much smaller number means the "
            + "scan stopped finding domain source rather than that the domain "
            + "shrank");
    }

    private static List<string> DomainSourceFiles() =>
        Repo.SourceFiles("src")
            .Where(path => Repo.Relative(path)
                .Contains(".Domain/", StringComparison.Ordinal))
            .ToList();
}
