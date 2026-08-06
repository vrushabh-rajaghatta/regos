using RegOS.Process.Domain.Aggregates.ProcessDefinitions;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Process.Domain.Aggregates.ProcessPlans;

/// <summary>
/// One live, dated step of a plan.
/// </summary>
/// <remarks>
/// <b>Where a <c>ProcessStepDefinition</c> describes work, this <em>is</em> the
/// work</b> — the same code and name, plus the two dates instantiation derived.
/// <para>
/// <b>It carries no execution state in S003.</b> No actual dates, no status, no
/// history: those arrive with S004, which is the story that models what working
/// a plan means. What exists here is a schedule, and the schedule is complete.
/// </para>
/// <para>
/// <b><see cref="PlannedStartOn"/> and <see cref="PlannedEndOn"/> are inclusive
/// calendar dates.</b> A five-day step starting on the 1st ends on the 5th,
/// which is what a person means by it. Stated because the alternative reading is
/// equally defensible and silently off by one.
/// </para>
/// </remarks>
public sealed class ProcessStep : Entity<ProcessStepId>
{
    private readonly List<ProcessStepDependency> _predecessors = [];
    private readonly List<ProcessStepStatusEntry> _history = [];

    // EF materialisation.
    private ProcessStep()
    {
    }

    internal ProcessStep(
        ProcessStepId id,
        ProcessStepDefinitionId stepDefinitionId,
        string code,
        string name,
        string? description,
        ProcessStepId? parentStepId,
        int order,
        DateOnly plannedStartOn,
        DateOnly plannedEndOn,
        DateOnly openedOn)
    {
        Id = id;
        StepDefinitionId = stepDefinitionId;
        Code = code;
        Name = name;
        Description = description;
        ParentStepId = parentStepId;
        Order = order;
        PlannedStartOn = plannedStartOn;
        PlannedEndOn = plannedEndOn;
        CurrentStatus = ProcessStepStatus.NotStarted;

        // openedOn, not plannedStartOn: the step became "not started" when the
        // plan was drawn up, not on the day it was scheduled to begin. Seeding
        // it with the planned start made the chronology rule forbid recording
        // work EARLY, which is an ordinary thing for a team to do — found by an
        // impact test in S005, one story after the mistake was made.
        _history.Add(new ProcessStepStatusEntry(
            ProcessStepStatusEntryId.New(),
            ProcessStepStatus.NotStarted,
            openedOn,
            DateTime.UtcNow,
            null));
    }

    /// <summary>
    /// The authored step this came from. Provenance, not a live link — the
    /// definition version is frozen, so this can never disagree with itself.
    /// </summary>
    public ProcessStepDefinitionId StepDefinitionId { get; private set; } = default!;

    public string Code { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    public string? Description { get; private set; }

    /// <summary>The step this one is part of, translated to <em>this plan's</em> ids.</summary>
    public ProcessStepId? ParentStepId { get; private set; }

    /// <summary>
    /// Display order among siblings, copied from the definition. Not unique —
    /// every read that sorts by it ends in a unique key as well.
    /// </summary>
    public int Order { get; private set; }

    /// <summary>Inclusive.</summary>
    public DateOnly PlannedStartOn { get; private set; }

    /// <summary>Inclusive. Never before <see cref="PlannedStartOn"/>.</summary>
    public DateOnly PlannedEndOn { get; private set; }

    /// <summary>
    /// What this step waits for, as <em>this plan's</em> step ids. The graph is
    /// copied at instantiation rather than read back through the definition, so a
    /// plan is readable without loading the playbook it came from.
    /// </summary>
    public IReadOnlyCollection<ProcessStepDependency> Predecessors
        => _predecessors.AsReadOnly();

    /// <summary>
    /// Stored, and it earns that: the "what is next" read filters on it across
    /// every step of every active plan, which would otherwise walk every history.
    /// </summary>
    public ProcessStepStatus CurrentStatus { get; private set; }

    public IReadOnlyList<ProcessStepStatusEntry> History => _history.AsReadOnly();

    /// <summary>
    /// When work actually began, or null if it never was marked as begun.
    /// </summary>
    /// <remarks>
    /// <b>Derived, never stored</b> — the call <c>Commitment.GivenOn</c> made and
    /// every dated history in RegOS has followed. <b>Null after a step went
    /// straight to complete is honest</b>, not a gap: nobody recorded a start, so
    /// RegOS does not know one.
    /// </remarks>
    public DateOnly? ActualStartOn
        => _history
            .Where(x => x.Status == ProcessStepStatus.InProgress)
            .Select(x => (DateOnly?)x.OccurredOn)
            .FirstOrDefault();

    /// <summary>When it finished, either way. Derived.</summary>
    public DateOnly? ActualEndOn
        => _history
            .Where(x => x.Status is ProcessStepStatus.Complete
                or ProcessStepStatus.Skipped)
            .Select(x => (DateOnly?)x.OccurredOn)
            .FirstOrDefault();

    /// <summary>Nothing further is expected of this step, either way.</summary>
    public bool IsSettled
        => CurrentStatus is ProcessStepStatus.Complete
            or ProcessStepStatus.Skipped;

    internal void WaitFor(ProcessStepId predecessorStepId)
        => _predecessors.Add(new ProcessStepDependency(predecessorStepId));

    /// <summary>
    /// Records a transition. <b>Called only by the plan</b>, which owns the rules
    /// about which transitions are legal — a step is a child entity and the
    /// aggregate root is the consistency boundary (ADR-016).
    /// </summary>
    internal void RecordStatus(
        ProcessStepStatus status, DateOnly occurredOn, string? note)
    {
        _history.Add(new ProcessStepStatusEntry(
            ProcessStepStatusEntryId.New(),
            status,
            occurredOn,
            DateTime.UtcNow,
            note));

        CurrentStatus = status;
    }

    /// <summary>The latest business date recorded, for the chronology rule.</summary>
    internal DateOnly LatestRecordedOn => _history.Max(entry => entry.OccurredOn);

    /// <summary>
    /// Moves the planned dates. <b>A current value, not history</b> — a schedule
    /// is intent a human owns, and I6 governs execution rather than intent.
    /// </summary>
    internal void Reschedule(DateOnly plannedStartOn, DateOnly plannedEndOn)
    {
        if (plannedEndOn < plannedStartOn)
            throw new DomainException(ProcessPlanErrors.EndBeforeStart);

        PlannedStartOn = plannedStartOn;
        PlannedEndOn = plannedEndOn;
    }
}
