using System.Text.RegularExpressions;

using FluentAssertions;

namespace RegOS.Architecture.Tests;

/// <summary>
/// <b>Every externally observable ordering must be deterministic</b> — given the
/// same database state, repeated execution returns rows in the same order.
/// </summary>
/// <remarks>
/// <para>
/// <b>The invariant is determinism, not ids.</b> An id is the easiest way to
/// achieve it and not the requirement, so this test accepts two proofs: an
/// ordering that <b>terminates in a unique key</b>, or one whose call site
/// <b>states the invariant that already makes it total</b>. Written this way on
/// purpose — if RegOS later prefers natural keys, or some other uniqueness
/// mechanism, the rule still describes the property it cares about rather than
/// today's technique.
/// </para>
/// <para>
/// <b>Why it exists.</b> <c>ListManufacturingOperations</c> ordered by
/// <em>(current, EffectiveFrom desc)</em> and nothing else. Two operations at one
/// site starting the same day tie on every key, and Postgres may return them
/// either way round — so a list reordered itself between reloads for no reason a
/// user could see. It surfaced as an intermittent browser failure that read
/// convincingly as environmental, and cost most of a session before it was
/// chased rather than re-run.
/// </para>
/// <para>
/// <b>Deliberately not narrowed to dates.</b> That defect happened to involve a
/// date; the property it violated was <em>partial ordering</em>, and tomorrow
/// that could be a name, a sequence number, a display order, a version or a
/// priority. Narrowing a rule to the shape of the last bug is how you get the
/// next one.
/// </para>
/// <para>
/// <b>Scoped to read paths</b> — classes that hold a <c>RegOSDbContext</c>. That
/// is where an ordering becomes part of what a caller observes. It deliberately
/// includes in-memory orderings in those classes: LINQ-to-Objects sorts
/// <em>stably</em>, so such an ordering is deterministic exactly when its source
/// is, and <b>that reasoning is knowledge worth writing down</b> rather than
/// re-deriving.
/// </para>
/// </remarks>
public class DeterministicOrderingTests
{
    /// <summary>
    /// The marker a call site uses to state why its ordering is already total.
    /// A structured prefix rather than free prose, because the rule has to be
    /// checkable — but everything after it is prose, and it is the sentence a
    /// reader actually needs.
    /// </summary>
    private const string Marker = "Deterministic:";

    /// <summary>
    /// How far above an ordering the justification may sit. Wide enough for a
    /// comment introducing a whole LINQ query, narrow enough that it cannot
    /// belong to something else.
    /// </summary>
    private const int JustificationWindow = 12;

    [Fact]
    public void Every_ordering_on_a_read_path_proves_its_determinism()
    {
        var offenders = Orderings()
            .Where(x => !x.EndsInUniqueKey && !x.HasJustification)
            .Select(x => $"{x.File}:{x.Line}  {x.Text}")
            .ToList();

        offenders.Should().BeEmpty(
            "an ordering whose keys can tie lets the database return either row "
            + "first, and the list reorders itself between reloads. End it with "
            + "a unique key, or write a `// " + Marker + " …` comment saying "
            + "what already makes it total");
    }

    /// <summary>
    /// The negative control. Without it the assertion above passes by reading
    /// nothing — the failure mode this repository has been bitten by when
    /// counting test suites.
    /// </summary>
    [Fact]
    public void The_read_paths_are_actually_being_read()
    {
        Orderings().Should().HaveCountGreaterThan(80,
            "RegOS had 124 orderings on read paths when this was written; a much "
            + "smaller number means the scan stopped matching rather than that "
            + "the queries stopped ordering");
    }

    private sealed record Ordering(
        string File, int Line, string Text,
        bool EndsInUniqueKey, bool HasJustification);

    private static readonly Regex QuerySyntax = new(
        @"^\s*orderby\s+(?<keys>.+?)\s*$", RegexOptions.Compiled);

    private static readonly Regex MethodSyntax = new(
        @"\.Order(By|ByDescending)\(", RegexOptions.Compiled);

    private static readonly Regex Continuation = new(
        @"^\s*\.Then(By|ByDescending)\(", RegexOptions.Compiled);

    /// <summary>
    /// A key is unique when it is an identity — <c>Id</c>, or anything ending in
    /// <c>Id</c>. Everything else has to say why.
    /// </summary>
    private static readonly Regex UniqueKey = new(
        @"(^|\.)\w*Id\s*\)?\s*$", RegexOptions.Compiled);

    private static List<Ordering> Orderings()
    {
        var found = new List<Ordering>();

        foreach (var path in Repo.SourceFiles("src"))
        {
            var text = File.ReadAllText(path);

            // Read paths only: a class that cannot reach the database cannot
            // order rows coming out of one.
            if (!text.Contains("RegOSDbContext", StringComparison.Ordinal))
                continue;

            var lines = text.Split('\n');

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                var isQuery = QuerySyntax.IsMatch(line);
                var isMethod = MethodSyntax.IsMatch(line);

                if (!isQuery && !isMethod) continue;

                // A .ThenBy on its own line is a continuation of the ordering
                // above it, not a new one.
                if (Continuation.IsMatch(line)) continue;

                found.Add(new Ordering(
                    Repo.Relative(path),
                    i + 1,
                    line.Trim(),
                    EndsInUniqueKey(lines, i),
                    Justified(lines, i)));
            }
        }

        return found;
    }

    /// <summary>
    /// The last key of the whole ordering — following <c>.ThenBy</c> chains and
    /// comma-separated query-syntax keys to whichever comes last.
    /// </summary>
    private static bool EndsInUniqueKey(string[] lines, int start)
    {
        var last = lines[start];

        // Query syntax spreads its keys over as many lines as it likes, and each
        // continuation line ends in a comma. Method syntax puts each key in its
        // own .ThenBy. Follow whichever this is to its last key.
        if (QuerySyntax.IsMatch(last))
        {
            for (var i = start; i < lines.Length && last.TrimEnd().EndsWith(','); i++)
                last = lines[i + 1];
        }

        for (var i = start + 1; i < lines.Length && Continuation.IsMatch(lines[i]); i++)
            last = lines[i];

        // Query syntax puts every key on one line; take the final one.
        var keys = QuerySyntax.Match(last) is { Success: true } match
            ? match.Groups["keys"].Value
            : last;

        var final = keys.Split(',')[^1]
            .Replace("descending", string.Empty, StringComparison.Ordinal)
            .Replace("ascending", string.Empty, StringComparison.Ordinal)
            .Trim();

        return UniqueKey.IsMatch(final);
    }

    private static bool Justified(string[] lines, int start)
    {
        for (var i = Math.Max(0, start - JustificationWindow); i <= start; i++)
        {
            if (lines[i].Contains(Marker, StringComparison.Ordinal))
                return true;
        }

        return false;
    }
}
