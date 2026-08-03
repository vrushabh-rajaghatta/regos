using System.Text.RegularExpressions;

using FluentAssertions;

namespace RegOS.Architecture.Tests;

/// <summary>
/// <b>EPIC-007a S005 — the check the folder column can never have.</b>
///
/// A folder name RegOS invents is legal XML and merely unconventional, which is
/// why <c>EctdFolderSource</c> has to record who chose it (ADR-052). An
/// <em>element</em> name RegOS invents is <b>invalid</b> — so the DTD the
/// package ships is itself the oracle, and no provenance is needed.
///
/// <para>
/// This is Level 2a applied to our own reference data: a third-party artifact we
/// did not write, checking values we did. It lives with the architecture tests
/// because it asserts a repository-wide invariant across two directories that
/// have no code dependency on each other — the blueprint seed and the pinned
/// specifications.
/// </para>
/// </summary>
public class SeededElementConventionTests
{
    private const string Seed =
        "src/Persistence/RegOS.Persistence/Initialization/ReferenceData/"
        + "Blueprint/RegulatoryTemplates.cs";

    /// <summary>
    /// Element names as the seed writes them, for both backbones. A value may
    /// chain on <c>/</c> where RegOS's tree is coarser than the CTD's.
    /// </summary>
    /// <remarks>
    /// <b>At least one hyphenated segment is required</b>, which is what
    /// separates an element from a folder: the seed writes <c>"m2"</c> for a
    /// directory and <c>"m2-common-technical-document-summaries"</c> for the
    /// element inside it. Without that, the bare module folders match and the
    /// test reports them as undeclared elements — which is how this regex first
    /// failed, and a good sign it is reading real values rather than nothing.
    /// </remarks>
    private static readonly Regex ElementLiteral = new(
        @"""(?<value>m[1-5](?:-[a-z0-9]+)+(?:/m[1-5](?:-[a-z0-9]+)+)*)""",
        RegexOptions.Compiled);

    private static readonly Regex Declaration = new(
        @"<!ELEMENT\s+(?<name>[a-z0-9:-]+)", RegexOptions.Compiled);

    private static HashSet<string> Declared(params string[] dtdFileNames) =>
        dtdFileNames
            .SelectMany(name => Declaration
                .Matches(File.ReadAllText(Path.Combine(
                    Repo.Root, "docs", "evidence", "EPIC-007a", "spec", name)))
                .Select(m => m.Groups["name"].Value))
            .ToHashSet();

    private static List<string> SeededElementNames() =>
        ElementLiteral
            .Matches(File.ReadAllText(Path.Combine(Repo.Root, Seed)))
            .Select(m => m.Groups["value"].Value)
            .SelectMany(value => value.Split('/'))
            .Distinct()
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();

    [Fact]
    public void Every_seeded_element_is_declared_in_a_pinned_dtd()
    {
        // Both backbones, because a section carries a name in each and the two
        // vocabularies do not overlap: ICH declares one Module 1 element, FDA
        // declares 147 (evidence E16 — a backbone is a contract).
        var declared = Declared("ich-ectd-3-2.dtd", "us-regional-v3-3.dtd");

        var undeclared = SeededElementNames()
            .Where(name => !declared.Contains(name))
            .ToList();

        undeclared.Should().BeEmpty(
            "an element name no DTD declares cannot appear in a valid package, "
            + "so seeding one is a defect rather than a style choice — check it "
            + "against docs/evidence/EPIC-007a/spec/");
    }

    /// <summary>
    /// The negative control. Without it, a regex that stopped matching would
    /// make the assertion above vacuously true — the failure mode this project
    /// has already been bitten by when counting test suites.
    /// </summary>
    [Fact]
    public void The_seed_actually_carries_element_names()
    {
        SeededElementNames().Should().HaveCountGreaterThan(30,
            "the FDA IND blueprint names ~40 sections across both backbones; a "
            + "much smaller number means the literals stopped being matched, "
            + "not that the seed shrank");
    }
}
