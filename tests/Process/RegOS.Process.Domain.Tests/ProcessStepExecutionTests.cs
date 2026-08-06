using FluentAssertions;

using RegOS.Process.Domain.Aggregates.ProcessDefinitions;
using RegOS.Process.Domain.Aggregates.ProcessObjectives;
using RegOS.Process.Domain.Aggregates.ProcessPlans;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Process.Domain.Tests;

/// <summary>
/// Working a plan — <b>and the rules that keep execution a decision rather than
/// a consequence</b> (ADR-065 D11, I6).
/// </summary>
public class ProcessStepExecutionTests
{
    private static readonly DateOnly Anchor = new(2026, 9, 1);

    [Fact]
    public void A_new_step_is_not_started_and_has_one_history_entry()
    {
        var plan = ActivePlan();
        var step = First(plan);

        step.CurrentStatus.Should().Be(ProcessStepStatus.NotStarted);
        step.History.Should().ContainSingle();
        step.ActualStartOn.Should().BeNull();
        step.ActualEndOn.Should().BeNull();
        step.IsSettled.Should().BeFalse();
    }

    [Fact]
    public void A_step_runs_from_not_started_to_complete()
    {
        var plan = ActivePlan();
        var step = First(plan);

        plan.StartStep(step.Id, Anchor.AddDays(1));
        plan.CompleteStep(step.Id, Anchor.AddDays(4), "Filed via ESG.");

        step.CurrentStatus.Should().Be(ProcessStepStatus.Complete);
        step.ActualStartOn.Should().Be(Anchor.AddDays(1));
        step.ActualEndOn.Should().Be(Anchor.AddDays(4));
        step.IsSettled.Should().BeTrue();
        step.History.Should().HaveCount(3);
    }

    /// <summary>
    /// Work finished that nobody marked as begun is ordinary. Refusing it would
    /// only teach people to record a fictional start date.
    /// </summary>
    [Fact]
    public void A_step_may_complete_without_ever_having_started()
    {
        var plan = ActivePlan();
        var step = First(plan);

        plan.CompleteStep(step.Id, Anchor.AddDays(2));

        step.CurrentStatus.Should().Be(ProcessStepStatus.Complete);
        step.ActualEndOn.Should().Be(Anchor.AddDays(2));
        step.ActualStartOn.Should().BeNull(
            "nobody recorded a start, so RegOS does not know one");
    }

    /// <summary>
    /// <b>The one place S004 adds friction on purpose.</b> "Skipped" on its own
    /// is not an explanation, and a year later somebody will ask.
    /// </summary>
    [Fact]
    public void A_step_cannot_be_skipped_without_a_reason()
    {
        var plan = ActivePlan();
        var step = First(plan);

        var silent = () => plan.SkipStep(step.Id, Anchor, "   ");

        silent.Should().Throw<DomainException>()
            .WithMessage(ProcessPlanErrors.SkipReasonRequired);
    }

    [Fact]
    public void A_skipped_step_keeps_its_reason_and_is_settled()
    {
        var plan = ActivePlan();
        var step = First(plan);

        plan.SkipStep(step.Id, Anchor.AddDays(3), "Not required for this route.");

        step.CurrentStatus.Should().Be(ProcessStepStatus.Skipped);
        step.IsSettled.Should().BeTrue();
        step.ActualEndOn.Should().Be(Anchor.AddDays(3));
        step.History[^1].Note.Should().Be("Not required for this route.");
    }

    /// <summary>
    /// Both open states reach <c>Skipped</c>: a team may decide before starting
    /// that a step does not apply, or discover it halfway through.
    /// </summary>
    [Fact]
    public void A_step_in_progress_may_still_be_skipped()
    {
        var plan = ActivePlan();
        var step = First(plan);

        plan.StartStep(step.Id, Anchor);
        plan.SkipStep(step.Id, Anchor.AddDays(2), "Superseded by FDA advice.");

        step.CurrentStatus.Should().Be(ProcessStepStatus.Skipped);
        step.History.Should().HaveCount(3);
    }

    [Fact]
    public void A_settled_step_cannot_be_moved_again()
    {
        var plan = ActivePlan();
        var step = First(plan);

        plan.CompleteStep(step.Id, Anchor);

        var reopen = () => plan.StartStep(step.Id, Anchor.AddDays(1));

        reopen.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ProcessPlanErrors.StepAlreadySettled);
    }

    /// <summary>I6 — a history is append-only, so it is also in order.</summary>
    [Fact]
    public void A_step_cannot_record_a_date_before_something_already_recorded()
    {
        var plan = ActivePlan();
        var step = First(plan);

        plan.StartStep(step.Id, Anchor.AddDays(5));

        var backwards = () => plan.CompleteStep(step.Id, Anchor.AddDays(4));

        backwards.Should().Throw<DomainException>()
            .WithMessage(ProcessPlanErrors.StepHistoryOutOfOrder);
    }

    [Fact]
    public void Work_cannot_be_recorded_against_a_draft_plan()
    {
        var plan = DraftPlan();

        var early = () => plan.StartStep(First(plan).Id, Anchor);

        early.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ProcessPlanErrors.NotActive);
    }

    /// <summary>
    /// <b>A plan completes on a judgement, not a count</b> (D11). Requiring every
    /// step to be <c>Complete</c> would push a team to mark work done that was
    /// not, in order to close the plan.
    /// </summary>
    [Fact]
    public void A_plan_completes_with_a_skipped_step_in_it()
    {
        var plan = ActivePlan();
        var steps = plan.Steps.OrderBy(x => x.Code, StringComparer.Ordinal).ToList();

        plan.CompleteStep(steps[0].Id, Anchor.AddDays(5));
        plan.SkipStep(steps[1].Id, Anchor.AddDays(6), "Not applicable.");

        var complete = () => plan.Complete(Anchor.AddDays(7));

        complete.Should().NotThrow();
        plan.CurrentStatus.Should().Be(ProcessPlanStatus.Completed);
    }

    /// <summary>
    /// And it completes with steps still open, for the same reason: the condition
    /// is <em>no further execution is expected</em>, which is a person's call.
    /// </summary>
    [Fact]
    public void A_plan_completes_even_with_open_steps()
    {
        var plan = ActivePlan();

        plan.Complete(Anchor.AddDays(30));

        plan.CurrentStatus.Should().Be(ProcessPlanStatus.Completed);
        plan.Steps.Should().Contain(x => !x.IsSettled);
    }

    [Fact]
    public void A_closed_plan_cannot_be_reopened_or_worked()
    {
        var plan = ActivePlan();
        plan.Cancel(Anchor.AddDays(2), "Programme deprioritised.");

        var reopen = () => plan.Complete(Anchor.AddDays(3));
        var work = () => plan.StartStep(First(plan).Id, Anchor.AddDays(3));

        reopen.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ProcessPlanErrors.AlreadyClosed);
        work.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ProcessPlanErrors.NotActive);
    }

    /// <summary>
    /// <b>Rescheduling is permitted and moves nothing else</b> (D5) — a planned
    /// date is a current value, not history. I6 governs execution, not intent.
    /// </summary>
    [Fact]
    public void Rescheduling_a_step_moves_only_that_step()
    {
        var plan = ActivePlan();
        var steps = plan.Steps.OrderBy(x => x.Code, StringComparer.Ordinal).ToList();
        var untouched = steps[1].PlannedStartOn;

        plan.RescheduleStep(
            steps[0].Id, Anchor.AddDays(10), Anchor.AddDays(20));

        steps[0].PlannedStartOn.Should().Be(Anchor.AddDays(10));
        steps[1].PlannedStartOn.Should().Be(untouched,
            "moving a step moves nothing else, by design");
        steps[0].History.Should().ContainSingle(
            "a schedule change is not an execution fact");
    }

    // --- fixtures ------------------------------------------------------------

    private static ProcessStep First(ProcessPlan plan)
        => plan.Steps.OrderBy(x => x.Code, StringComparer.Ordinal).First();

    private static ProcessPlan ActivePlan()
    {
        var plan = DraftPlan();
        plan.Activate(Anchor);

        return plan;
    }

    private static ProcessPlan DraftPlan()
    {
        var definition = ProcessDefinitionTests.ADefinition();
        var version = definition.StartDraftVersion();
        var first = definition.AddStep("A", "First", durationDays: 5);
        var second = definition.AddStep("B", "Second", durationDays: 3);
        definition.AddStepPredecessor(second.Id, first.Id);
        definition.PublishVersion(version.Id, null, DateTime.UtcNow);

        return ProcessPlan.InstantiateFrom(
            new TenantId(Guid.NewGuid()),
            ProcessObjectiveId.New(),
            version,
            Anchor,
            "US IND filing plan",
            Anchor);
    }
}
