using System.Text.RegularExpressions;

using FluentAssertions;

namespace RegOS.Architecture.Tests;

/// <summary>
/// The watchpoint EPIC-018 D2 accepted: <b>carton artwork is a local label
/// type, and stays one only while every invariant applies equally to every
/// type.</b>
/// </summary>
/// <remarks>
/// The decision was to keep artwork inside <c>LocalLabel</c> rather than give it
/// its own root — one revision lifecycle, one approval model, one effective
/// dating, one API, one browser proof — at the cost of a nullable column or two.
/// <para>
/// <b>Nullable columns are not the signal that the trade has stopped paying.</b>
/// <c>AtcCode</c> has been one on <c>MedicinalProduct</c> since EPIC-017 and has
/// cost nobody anything. The signal is <em>branching</em>: the moment the domain
/// reads <c>if (Type == Artwork)</c>, artwork has acquired its own invariants
/// and is a different aggregate wearing a shared one's clothes.
/// </para>
/// <para>
/// This test exists so that question is asked by the build rather than by
/// whoever happens to be reading the code a year from now. A failure here is
/// <b>not necessarily a defect</b> — it is the conversation about splitting
/// <c>CartonArtwork</c> out, arriving at the moment the evidence does.
/// </para>
/// </remarks>
public sealed class LocalLabelTypeBranchTests
{
    /// <summary>
    /// Code comparisons against a label type, and switches over one. Deliberately
    /// broad: a false positive costs one reading, and a missed branch costs the
    /// decision this test exists to protect.
    /// </summary>
    private static readonly Regex TypeBranch = new(
        """(LabelType\s*\.\s*Code\s*==)|(==\s*"(ARTWORK|SMPC|PIL|CONTAINER)")|(switch\s*\(\s*[A-Za-z_.]*LabelType)""",
        RegexOptions.Compiled);

    [Fact]
    public void The_labeling_domain_does_not_branch_on_a_local_label_type()
    {
        var branching = Repo.SourceFiles("src/Labeling")
            .Where(path => Repo.Relative(path).Contains(
                ".Domain/", StringComparison.Ordinal))
            .SelectMany(path => File
                .ReadAllLines(path)
                .Select((line, index) => (path, line, number: index + 1)))
            .Where(x => TypeBranch.IsMatch(x.line))
            // A comment naming the pattern is how the decision is documented in
            // LocalLabel and LabelVocabulary; it is the code that matters.
            .Where(x => !x.line.TrimStart().StartsWith("//", StringComparison.Ordinal)
                     && !x.line.TrimStart().StartsWith("///", StringComparison.Ordinal)
                     && !x.line.TrimStart().StartsWith("*", StringComparison.Ordinal))
            .Select(x => $"{Repo.Relative(x.path)}:{x.number}")
            .ToList();

        branching.Should().BeEmpty(
            "EPIC-018 D2 keeps carton artwork inside LocalLabel only while every "
            + "invariant applies equally to every type. A branch on the type is "
            + "the aggregate asking to split — read the watchpoint in "
            + "docs/product/epics/EPIC-018-labeling-and-product-information.md "
            + "and either justify the branch or extract CartonArtwork. Do not "
            + "silence this test.");
    }
}
