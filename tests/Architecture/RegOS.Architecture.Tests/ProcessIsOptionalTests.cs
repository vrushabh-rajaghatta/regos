using System.Text.RegularExpressions;

using FluentAssertions;

namespace RegOS.Architecture.Tests;

/// <summary>
/// <b>ADR-065 I3, which until now was a diagram.</b>
/// </summary>
/// <remarks>
/// <para>
/// I3 draws six contexts each holding one optional arrow into <c>ProcessStep</c>,
/// and says <em>each context owns itself; nothing requires Process</em>. Every
/// word of that was true when written and none of it was checked — the
/// dependency graph asserts which contexts <b>may</b> reference Process, and
/// says nothing about the <b>shape</b> the reference has to take.
/// </para>
/// <para>
/// <b>Both rules below held with zero violations on the day they were written</b>
/// (S008's audit), across all six integrating contexts. That is the point: the
/// value of freezing something already true is entirely in the first attempt to
/// break it, and both of these break by convenience —
/// <c>ProcessStepId</c> made non-nullable "because every submission has one now",
/// or a domain reaching for <c>ProcessPlan</c> to read a date off it.
/// </para>
/// <para>
/// <b>Named for Process rather than for optional contexts in general.</b> Process
/// is the only one, and a general mechanism for a single case is the speculation
/// <see href="../../../docs/adr/ADR-018-rule-of-three.md">ADR-018</see> forbids.
/// </para>
/// </remarks>
public class ProcessIsOptionalTests
{
    /// <summary>
    /// <b>Every integration is nullable</b> — the half of I1 that is structural.
    /// </summary>
    /// <remarks>
    /// A required <c>ProcessStepId</c> anywhere would mean that context could no
    /// longer record anything without a plan, and Process would have stopped
    /// being optional in exactly the way I1 forbids. The database says the same
    /// thing — all six columns are nullable with <c>SetNull</c> — but a
    /// non-nullable property with a nullable column is a model that lies, so the
    /// check belongs on the declaration.
    /// </remarks>
    [Fact]
    public void Every_reference_to_a_process_step_is_nullable()
    {
        var offenders = new List<string>();

        foreach (var (relative, source) in SourceOutsideProcess())
        {
            foreach (Match match in StepIdProperty.Matches(source))
            {
                if (!match.Groups["nullable"].Success)
                    offenders.Add($"{relative}: {match.Value.Trim()}");
            }
        }

        offenders.Should().BeEmpty(
            "ADR-065 I1 and I3 — a required ProcessStepId would make Process "
            + "mandatory for that context, and RegOS must behave identically "
            + "with an empty Process schema");
    }

    /// <summary>
    /// <b>And an id is the <em>only</em> thing that crosses.</b>
    /// </summary>
    /// <remarks>
    /// The arrow in I3's diagram points at <c>ProcessStep</c> and carries an
    /// identifier — not a plan, not a definition, not a status. A domain naming
    /// <c>ProcessPlan</c> could read a date off it, and then two contexts would
    /// hold the same schedule; naming <c>ProcessStepStatus</c> would let a
    /// registration decide what a step's status means. Both are the same mistake
    /// as a navigation property (ES-014), one layer up.
    /// <para>
    /// <b>Scoped to <c>*.Domain</c> deliberately.</b> An application handler
    /// legitimately reads <c>_dbContext.ProcessPlans</c> to check a step exists —
    /// ADR-016 grants that read, and I2's repository guard is what keeps it a
    /// read. Ownership lives in the domain, so that is where this looks.
    /// </para>
    /// </remarks>
    [Fact]
    public void A_domain_outside_process_names_no_process_type_but_the_step_id()
    {
        var offenders = new List<string>();

        foreach (var (relative, source) in SourceOutsideProcess())
        {
            if (!relative.Contains(".Domain/", StringComparison.Ordinal))
                continue;

            foreach (Match match in ProcessType.Matches(source))
            {
                var name = match.Groups["type"].Value;

                // The namespace segment the using directive ends in, not a type.
                if (name is "ProcessStepId" or "ProcessPlans")
                    continue;

                offenders.Add($"{relative} names {name}");
            }
        }

        offenders.Should().BeEmpty(
            "ADR-065 I3 — what crosses into another context is an identifier. A "
            + "domain that can see a ProcessPlan can read a schedule off it, and "
            + "then two contexts hold the same dates");
    }

    // --- source scanning ---------------------------------------------------

    /// <summary><c>public ProcessStepId? ProcessStepId { get; private set; }</c></summary>
    private static readonly Regex StepIdProperty = new(
        @"^\s*public\s+ProcessStepId(?<nullable>\?)?\s+\w+\s*\{",
        RegexOptions.Compiled | RegexOptions.Multiline);

    /// <summary>Any <c>Process*</c> identifier, however it is spelled.</summary>
    private static readonly Regex ProcessType = new(
        @"\b(?<type>Process[A-Z]\w*)\b", RegexOptions.Compiled);

    /// <summary>
    /// Every <c>.cs</c> file under <c>src/</c> that Process does not own, minus
    /// generated output. Persistence is excluded — its EF configuration names
    /// <c>ProcessStep</c> to declare the foreign key, which is the mapping doing
    /// its job, and its migrations are generated.
    /// </summary>
    private static IEnumerable<(string Relative, string Source)> SourceOutsideProcess()
    {
        foreach (var file in Directory.EnumerateFiles(
                     Path.Combine(Repo.Root, "src"), "*.cs",
                     SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(Repo.Root, file)
                .Replace('\\', '/');

            if (relative.Contains("/obj/", StringComparison.Ordinal)
                || relative.Contains("/bin/", StringComparison.Ordinal)
                || relative.StartsWith("src/Process/", StringComparison.Ordinal)
                || relative.StartsWith("src/Persistence/", StringComparison.Ordinal))
                continue;

            yield return (relative, File.ReadAllText(file));
        }
    }
}
