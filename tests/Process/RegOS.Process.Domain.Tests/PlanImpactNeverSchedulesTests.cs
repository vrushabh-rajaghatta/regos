using System.Reflection;

using FluentAssertions;

using RegOS.Process.Domain.Aggregates.ProcessPlans;

namespace RegOS.Process.Domain.Tests;

/// <summary>
/// <b>ADR-065 I7 — impact analysis never repairs the schedule.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>Split out of I8's test by S008's evidence audit</b>, which found one test
/// carrying both invariants and citing only one. They forbid different failures
/// and a future implementation could commit either alone:
/// </para>
/// <list type="table">
/// <item><term>I7</term><description>do not become a <b>scheduler</b> — never propose what to move</description></item>
/// <item><term>I8</term><description>do not become the <b>authoritative schedule</b> — never persist what you computed</description></item>
/// </list>
/// <para>
/// I8's half is covered where it has always been —
/// <c>PlanImpactReadTests.Running_the_analysis_changes_nothing_about_the_plan</c>
/// proves nothing is written. <b>This file proves nothing is suggested</b>, which
/// needs no database at all: it is a property of the arithmetic.
/// </para>
/// <para>
/// <b>The drift it forbids arrives as helpfulness.</b> A projection that has just
/// worked out a plan finishes nine days late is one small step from compressing a
/// downstream step to recover them — and at that moment RegOS has silently
/// acquired a scheduling engine that ADR-065's Out of scope list refuses.
/// </para>
/// </remarks>
public class PlanImpactNeverSchedulesTests
{
    private static readonly DateOnly Anchor = new(2026, 9, 1);

    /// <summary>
    /// <b>The behavioural half.</b> A step that has not started keeps its full
    /// planned duration in the projection, however late the plan is running.
    /// </summary>
    /// <remarks>
    /// <b>This is the assertion a scheduler fails.</b> Recovering a slip means
    /// shortening something, and the only things available to shorten are the
    /// steps that have not started yet. Holding every duration means the
    /// projection can only ever report the damage — never absorb it.
    /// </remarks>
    [Fact]
    public void A_delay_never_shortens_the_work_that_follows_it()
    {
        var steps = ThreeStepsInALine();

        // Sixty days after the anchor and the first step has still not started:
        // a serious slip, and the most tempting possible moment to help.
        var projection = PlanImpact.Project(steps, Anchor, Anchor.AddDays(60));

        foreach (var step in steps)
        {
            var projected = projection.Steps[step.Id];

            var plannedDays = step.PlannedEndOn.DayNumber
                - step.PlannedStartOn.DayNumber;

            var projectedDays = projected.ProjectedEndOn.DayNumber
                - projected.ProjectedStartOn.DayNumber;

            projectedDays.Should().Be(plannedDays,
                because: $"{step.Code} keeps the duration somebody planned for "
                    + "it — a projection that compressed it to recover the slip "
                    + "would be proposing a schedule (ADR-065 I7)");
        }
    }

    /// <summary>
    /// And the slip is <b>reported</b>, not absorbed. The finish moves out by the
    /// full amount rather than being held at the planned date.
    /// </summary>
    [Fact]
    public void The_finish_moves_out_rather_than_being_recovered()
    {
        var steps = ThreeStepsInALine();

        var onTime = PlanImpact.Project(steps, Anchor, Anchor);
        var late = PlanImpact.Project(steps, Anchor, Anchor.AddDays(60));

        late.SlipDays.Should().BeGreaterThan(0);

        late.ProjectedFinishOn.Should().BeAfter(onTime.ProjectedFinishOn!.Value,
            "the honest answer to \"if nothing changes\" is a later date. A "
                + "scheduler would answer with a different plan");

        late.PlannedFinishOn.Should().Be(onTime.PlannedFinishOn,
            "and the plan's own finish date is untouched by having been asked");
    }

    /// <summary>
    /// <b>The structural half.</b> Nothing the projection returns could carry a
    /// suggestion, because there is no member for one to live in.
    /// </summary>
    /// <remarks>
    /// Cheap, and it catches the version of this mistake that arrives as a
    /// well-meaning field rather than as changed arithmetic — a
    /// <c>SuggestedStartOn</c> or a <c>RecommendedDuration</c> added to be
    /// helpful. The projection may say <em>what will happen</em>; it may never
    /// say <em>what to do</em>.
    /// </remarks>
    [Fact]
    public void The_projection_exposes_no_member_that_could_be_a_proposal()
    {
        string[] proposalWords =
            ["suggest", "recommend", "propose", "revise", "adjust", "rebase",
             "optimi", "reschedul", "recover", "compress"];

        var offenders = new List<string>();

        foreach (var type in new[]
                 {
                     typeof(PlanProjection), typeof(ProjectedStep),
                     typeof(ScheduledStep)
                 })
        {
            foreach (var member in type.GetMembers(
                         BindingFlags.Public | BindingFlags.Instance))
            {
                if (proposalWords.Any(word => member.Name.Contains(
                        word, StringComparison.OrdinalIgnoreCase)))
                    offenders.Add($"{type.Name}.{member.Name}");
            }
        }

        offenders.Should().BeEmpty(
            "ADR-065 I7 — impact analysis answers \"if nothing changes…\" and "
                + "only that. A member naming a remedy is a scheduler arriving "
                + "one field at a time");
    }

    // --- fixtures ------------------------------------------------------------

    /// <summary>
    /// <c>A → B → C</c>, durations 5, 10 and 3, none of them started. A chain
    /// rather than a diamond: slack would let a projection hold the finish date
    /// honestly, and this file is about what happens when it cannot.
    /// </summary>
    private static List<ScheduledStep> ThreeStepsInALine()
    {
        var a = new ProcessStepId(Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"));
        var b = new ProcessStepId(Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002"));
        var c = new ProcessStepId(Guid.Parse("cccccccc-0000-0000-0000-000000000003"));

        return
        [
            new ScheduledStep(
                a, "A", Anchor, Anchor.AddDays(4), null, null,
                ProcessStepStatus.NotStarted, []),
            new ScheduledStep(
                b, "B", Anchor.AddDays(5), Anchor.AddDays(14), null, null,
                ProcessStepStatus.NotStarted, [a]),
            new ScheduledStep(
                c, "C", Anchor.AddDays(15), Anchor.AddDays(17), null, null,
                ProcessStepStatus.NotStarted, [b])
        ];
    }
}
