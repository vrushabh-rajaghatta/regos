using FluentAssertions;

using RegOS.Process.Domain.Aggregates.ProcessDefinitions;
using RegOS.Process.Domain.Aggregates.ProcessObjectives;
using RegOS.Process.Domain.Aggregates.ProcessPlans;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Process.Domain.Tests;

/// <summary>
/// <em>"Given what has happened, what does it now mean?"</em>
/// </summary>
/// <remarks>
/// <b>The case these tests exist for is slack</b> — a step running late inside
/// somebody else's critical path moves nothing, and the naive answer *(the step
/// is nine days late, so the plan is)* would be wrong. EPIC-020 S003 proved that
/// is not hypothetical: the FDA IND critical path runs through the meeting track,
/// not the 150-day CMC package, so the naive metric would be wrong by 33 days on
/// the one playbook RegOS ships.
/// </remarks>
public class PlanImpactTests
{
    private static readonly DateOnly Anchor = new(2026, 9, 1);

    /// <summary>Nothing has happened and nothing is late: the plan is unchanged.</summary>
    [Fact]
    public void An_untouched_plan_projects_to_its_planned_finish()
    {
        var plan = ADiamond();

        var projection = PlanImpact.Project(plan, Anchor);

        projection.SlipDays.Should().Be(0);
        projection.ProjectedFinishOn.Should().Be(projection.PlannedFinishOn);
    }

    /// <summary>
    /// <b>The headline case.</b> QUICK has 38 days of slack behind SLOW, so
    /// running nine days late costs the plan nothing.
    /// </summary>
    /// <remarks>
    /// SLOW is marked in progress on time. Without that it would be treated as
    /// not started and therefore late itself — which is correct behaviour and
    /// would drown the case this test is about.
    /// </remarks>
    [Fact]
    public void A_late_step_with_slack_moves_the_finish_by_nothing()
    {
        var plan = ADiamond();
        var plannedFinish = plan.Steps.Max(x => x.PlannedEndOn);

        plan.Activate(Anchor);
        plan.StartStep(plan.Steps.Single(x => x.Code == "SLOW").Id, Anchor);

        // QUICK was due to end 2 Sep. Ask on the 11th — nine days late.
        var projection = PlanImpact.Project(plan, new DateOnly(2026, 9, 11));

        projection.SlipDays.Should().Be(0,
            "the delay fell inside the slack behind the longer strand");
        projection.ProjectedFinishOn.Should().Be(plannedFinish);
    }

    /// <summary>
    /// And the same delay on the critical strand moves the finish by exactly as
    /// much — no special case, it falls out of the same walk.
    /// </summary>
    [Fact]
    public void A_late_step_on_the_critical_path_moves_the_finish()
    {
        var plan = ADiamond();

        plan.Activate(Anchor);
        plan.CompleteStep(plan.Steps.Single(x => x.Code == "QUICK").Id, Anchor.AddDays(1));
        plan.StartStep(plan.Steps.Single(x => x.Code == "SLOW").Id, Anchor);

        // SLOW was due to end 10 Oct. Ask on the 19th — nine days late.
        var projection = PlanImpact.Project(plan, new DateOnly(2026, 10, 19));

        projection.SlipDays.Should().Be(9);
        projection.ProjectedFinishOn.Should().Be(
            projection.PlannedFinishOn!.Value.AddDays(9));
    }

    /// <summary>Finishing early is not a negative slip; the plan simply holds.</summary>
    [Fact]
    public void Completing_work_early_never_reports_a_negative_slip()
    {
        var plan = ADiamond();
        plan.Activate(Anchor);

        foreach (var step in plan.Steps.Where(x => x.Code is "QUICK" or "SLOW"))
            plan.CompleteStep(step.Id, Anchor);

        var projection = PlanImpact.Project(plan, Anchor);

        projection.SlipDays.Should().Be(0);
    }

    /// <summary>A settled step's dates are facts; a projection may not argue with them.</summary>
    [Fact]
    public void A_settled_steps_actual_dates_are_authoritative()
    {
        var plan = ADiamond();
        plan.Activate(Anchor);

        var quick = plan.Steps.Single(x => x.Code == "QUICK");
        plan.CompleteStep(quick.Id, new DateOnly(2026, 9, 20));

        var projection = PlanImpact.Project(plan, new DateOnly(2026, 9, 25));

        var projectedQuick = projection.Steps[quick.Id];
        projectedQuick.ProjectedEndOn.Should().Be(new DateOnly(2026, 9, 20));
        projectedQuick.IsSettled.Should().BeTrue();
    }

    // --- the traversal -------------------------------------------------------

    /// <summary>
    /// <b>Downstream follows dependency, never execution.</b> Skipping a step
    /// says we did not perform it; it does not say the steps after it stopped
    /// depending on what came before.
    /// </summary>
    [Fact]
    public void Downstream_walks_through_a_skipped_step()
    {
        var plan = AChain();
        plan.Activate(Anchor);

        var b = plan.Steps.Single(x => x.Code == "B");
        plan.SkipStep(b.Id, Anchor, "Not required on this route.");

        var affected = PlanImpact.Downstream(
            plan, plan.Steps.Single(x => x.Code == "A").Id);

        var codes = affected
            .Select(id => plan.Steps.Single(x => x.Id == id).Code)
            .OrderBy(code => code, StringComparer.Ordinal);

        // Equal(IEnumerable, because), not Equal(params) — the latter would read
        // the reason as a third expected element.
        codes.Should().Equal(
            new[] { "B", "C" },
            (left, right) => left == right,
            "topology is topology — B is affected, and C still depends on A");
    }

    [Fact]
    public void Downstream_of_a_leaf_is_empty()
    {
        var plan = AChain();

        PlanImpact.Downstream(plan, plan.Steps.Single(x => x.Code == "C").Id)
            .Should().BeEmpty();
    }

    /// <summary>A converging graph reports each affected step once, not per path.</summary>
    [Fact]
    public void Downstream_reports_each_step_once_however_many_paths_reach_it()
    {
        var plan = ADiamond();

        PlanImpact.Downstream(plan, plan.Steps.Single(x => x.Code == "QUICK").Id)
            .Should().HaveCount(1);
    }

    // --- fixtures ------------------------------------------------------------

    /// <summary>QUICK (2d) and SLOW (40d) both from the anchor, joining at JOIN.</summary>
    private static ProcessPlan ADiamond()
    {
        var definition = ProcessDefinitionTests.ADefinition();
        var version = definition.StartDraftVersion();
        var quick = definition.AddStep("QUICK", "Quick", durationDays: 2);
        var slow = definition.AddStep("SLOW", "Slow", durationDays: 40);
        var join = definition.AddStep("JOIN", "Join", durationDays: 1);
        definition.AddStepPredecessor(join.Id, quick.Id);
        definition.AddStepPredecessor(join.Id, slow.Id);
        definition.PublishVersion(version.Id, null, DateTime.UtcNow);

        return Instantiate(version);
    }

    /// <summary>A → B → C.</summary>
    private static ProcessPlan AChain()
    {
        var definition = ProcessDefinitionTests.ADefinition();
        var version = definition.StartDraftVersion();
        var a = definition.AddStep("A", "First", durationDays: 2);
        var b = definition.AddStep("B", "Second", durationDays: 2);
        var c = definition.AddStep("C", "Third", durationDays: 2);
        definition.AddStepPredecessor(b.Id, a.Id);
        definition.AddStepPredecessor(c.Id, b.Id);
        definition.PublishVersion(version.Id, null, DateTime.UtcNow);

        return Instantiate(version);
    }

    private static ProcessPlan Instantiate(ProcessDefinitionVersion version)
        => ProcessPlan.InstantiateFrom(
            new TenantId(Guid.NewGuid()),
            ProcessObjectiveId.New(),
            version,
            Anchor,
            "Impact fixture",
            Anchor);
}
