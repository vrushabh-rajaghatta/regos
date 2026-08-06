using FluentAssertions;

using RegOS.Process.Domain.Aggregates.ProcessDefinitions;
using RegOS.Process.Domain.Aggregates.ProcessObjectives;
using RegOS.Process.Domain.Aggregates.ProcessPlans;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Process.Domain.Tests;

/// <summary>
/// Instantiation — <b>where three stories' decisions meet, and the first place
/// they can be falsified.</b>
/// </summary>
/// <remarks>
/// Every test here runs with no database, which is itself the point: the
/// derivation is a pure function of a frozen version, an objective and an anchor
/// date (ADR-065 D5).
/// </remarks>
public class ProcessPlanInstantiationTests
{
    private static readonly DateOnly Anchor = new(2026, 9, 1);

    /// <summary>
    /// <b>I4 enforced at the consuming end.</b> A draft is still being written
    /// and a superseded version is no longer instantiated from — and it is this
    /// refusal that makes publication's certificate applicable.
    /// </summary>
    [Fact]
    public void A_plan_cannot_be_instantiated_from_an_unpublished_version()
    {
        var definition = ProcessDefinitionTests.ADefinition();
        var draft = definition.StartDraftVersion();
        definition.AddStep("A", "First");

        var instantiate = () => Instantiate(draft);

        instantiate.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ProcessPlanErrors.VersionNotPublished);
    }

    [Fact]
    public void A_plan_belongs_to_an_objective_and_pins_its_version()
    {
        var version = APublishedVersion();

        var plan = Instantiate(version);

        plan.ProcessDefinitionVersionId.Should().Be(version.Id);
        plan.ProcessObjectiveId.Should().NotBeNull();
        plan.AnchorDate.Should().Be(Anchor);
        plan.CurrentStatus.Should().Be(ProcessPlanStatus.Draft);
    }

    /// <summary>
    /// The arithmetic, on the simplest possible shape. Inclusive dates: five days
    /// from the 1st ends on the 5th.
    /// </summary>
    [Fact]
    public void A_step_with_no_predecessors_starts_at_the_anchor_plus_its_offset()
    {
        var definition = ProcessDefinitionTests.ADefinition();
        var version = definition.StartDraftVersion();
        definition.AddStep("A", "First", durationDays: 5);
        definition.AddStep("B", "Later", offsetDays: 10, durationDays: 1);
        definition.PublishVersion(version.Id, null, DateTime.UtcNow);

        var plan = Instantiate(version);

        var first = plan.Steps.Single(x => x.Code == "A");
        first.PlannedStartOn.Should().Be(new DateOnly(2026, 9, 1));
        first.PlannedEndOn.Should().Be(new DateOnly(2026, 9, 5));

        plan.Steps.Single(x => x.Code == "B")
            .PlannedStartOn.Should().Be(new DateOnly(2026, 9, 11));
    }

    /// <summary>
    /// <c>OffsetDays = 0</c> means <em>"the day after the thing it waits for
    /// finishes"</em> — the convention the seed's screen text calls
    /// <em>"immediately after"</em>.
    /// </summary>
    [Fact]
    public void A_successor_starts_the_day_after_its_predecessor_ends()
    {
        var definition = ProcessDefinitionTests.ADefinition();
        var version = definition.StartDraftVersion();
        var first = definition.AddStep("A", "First", durationDays: 5);
        var second = definition.AddStep("B", "Second", durationDays: 3);
        definition.AddStepPredecessor(second.Id, first.Id);
        definition.PublishVersion(version.Id, null, DateTime.UtcNow);

        var plan = Instantiate(version);

        var b = plan.Steps.Single(x => x.Code == "B");
        b.PlannedStartOn.Should().Be(new DateOnly(2026, 9, 6));
        b.PlannedEndOn.Should().Be(new DateOnly(2026, 9, 8));
    }

    /// <summary>
    /// <b>The converging shape the FDA IND playbook actually has.</b> A step that
    /// waits for three strands starts after the <em>latest</em> of them — the
    /// case a naive walk gets wrong by taking the first, or the shortest.
    /// </summary>
    [Fact]
    public void A_step_waits_for_the_latest_of_its_predecessors()
    {
        var definition = ProcessDefinitionTests.ADefinition();
        var version = definition.StartDraftVersion();
        var quick = definition.AddStep("QUICK", "Quick", durationDays: 2);
        var slow = definition.AddStep("SLOW", "Slow", durationDays: 40);
        var join = definition.AddStep("JOIN", "Join", durationDays: 1);
        definition.AddStepPredecessor(join.Id, quick.Id);
        definition.AddStepPredecessor(join.Id, slow.Id);
        definition.PublishVersion(version.Id, null, DateTime.UtcNow);

        var plan = Instantiate(version);

        // SLOW runs 1 Sep – 10 Oct, so JOIN starts 11 Oct, not 3 Sep.
        plan.Steps.Single(x => x.Code == "JOIN")
            .PlannedStartOn.Should().Be(new DateOnly(2026, 10, 11));
    }

    /// <summary>
    /// <b>ADR-065 I5.</b> The same version, objective and anchor always produce
    /// the same plan — proven by rerunning it, not by inspecting the code.
    /// </summary>
    [Fact]
    public void Instantiation_is_deterministic()
    {
        var version = APublishedVersion();
        var objective = ProcessObjectiveId.New();
        var tenant = new TenantId(Guid.NewGuid());

        var first = ProcessPlan.InstantiateFrom(
            tenant, objective, version, Anchor, "Plan", Anchor);
        var second = ProcessPlan.InstantiateFrom(
            tenant, objective, version, Anchor, "Plan", Anchor);

        Shape(first).Should().BeEquivalentTo(Shape(second),
            because: "no clock, no database ordering and no randomness may reach "
                + "the schedule — only the ids differ between two runs");
    }

    /// <summary>The graph is copied into the plan's own identity space.</summary>
    [Fact]
    public void The_predecessor_graph_is_translated_to_this_plans_step_ids()
    {
        var definition = ProcessDefinitionTests.ADefinition();
        var version = definition.StartDraftVersion();
        var first = definition.AddStep("A", "First");
        var second = definition.AddStep("B", "Second");
        definition.AddStepPredecessor(second.Id, first.Id);
        definition.PublishVersion(version.Id, null, DateTime.UtcNow);

        var plan = Instantiate(version);

        var live = plan.Steps.Single(x => x.Code == "A");

        plan.Steps.Single(x => x.Code == "B")
            .Predecessors.Should().ContainSingle()
            .Which.PredecessorStepId.Should().Be(live.Id,
                "a plan is readable without loading the playbook it came from");
    }

    [Fact]
    public void Parent_steps_are_translated_too()
    {
        var definition = ProcessDefinitionTests.ADefinition();
        var version = definition.StartDraftVersion();
        var phase = definition.AddStep("PHASE", "Pre-IND phase");
        definition.AddStep("REQ", "Request", parentStepId: phase.Id);
        definition.PublishVersion(version.Id, null, DateTime.UtcNow);

        var plan = Instantiate(version);

        plan.Steps.Single(x => x.Code == "REQ").ParentStepId
            .Should().Be(plan.Steps.Single(x => x.Code == "PHASE").Id);
    }

    /// <summary>
    /// A plan becomes active when a team commits to working it — and a
    /// <em>Proposed</em> objective carrying a <em>Draft</em> plan is the normal
    /// state of an organisation still weighing its options.
    /// </summary>
    [Fact]
    public void A_draft_plan_becomes_active_once()
    {
        var plan = Instantiate(APublishedVersion());

        plan.Activate(Anchor.AddDays(7), "Funded at portfolio review.");

        plan.CurrentStatus.Should().Be(ProcessPlanStatus.Active);
        plan.History.Should().HaveCount(2);

        var again = () => plan.Activate(Anchor.AddDays(8));

        again.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ProcessPlanErrors.AlreadyActive);
    }

    // --- helpers -------------------------------------------------------------

    private static object Shape(ProcessPlan plan)
        => plan.Steps
            .OrderBy(step => step.Code, StringComparer.Ordinal)
            .Select(step => new
            {
                step.Code,
                step.PlannedStartOn,
                step.PlannedEndOn,
                step.Order,
                PredecessorCodes = step.Predecessors
                    .Select(x => plan.Steps.Single(s => s.Id == x.PredecessorStepId).Code)
                    .OrderBy(code => code, StringComparer.Ordinal)
                    .ToList()
            })
            .ToList();

    private static ProcessPlan Instantiate(ProcessDefinitionVersion version)
        => ProcessPlan.InstantiateFrom(
            new TenantId(Guid.NewGuid()),
            ProcessObjectiveId.New(),
            version,
            Anchor,
            "US IND filing plan",
            Anchor);

    private static ProcessDefinitionVersion APublishedVersion()
    {
        var definition = ProcessDefinitionTests.ADefinition();
        var version = definition.StartDraftVersion();

        var request = definition.AddStep("REQ", "Request", durationDays: 5);
        var package = definition.AddStep(
            "PKG", "Package", offsetDays: 30, durationDays: 30);
        var meeting = definition.AddStep("MTG", "Meeting", offsetDays: 30);

        definition.AddStepPredecessor(package.Id, request.Id);
        definition.AddStepPredecessor(meeting.Id, package.Id);

        definition.PublishVersion(version.Id, null, DateTime.UtcNow);

        return version;
    }
}
