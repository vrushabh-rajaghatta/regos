using RegOS.Process.Domain.Aggregates.ProcessDefinitions;
using RegOS.Process.Domain.Aggregates.ProcessObjectives;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Process.Domain.Aggregates.ProcessPlans;

/// <summary>
/// How we are going to achieve an objective, and by when.
/// </summary>
/// <remarks>
/// <b>A plan belongs to exactly one objective, and the link is required</b>
/// ([ADR-065](../../../../../docs/adr/ADR-065-regulatory-process-is-an-optional-bounded-context.md)
/// decision 3). Delete the plan and the intended outcome, the market, the
/// rationale and the ownership all survive; delete the objective and what is left
/// is a schedule that cannot say what it is for. <b>That is not a regulatory
/// plan, and RegOS is not a project-management tool</b> — so the model refuses to
/// hold one.
/// <para>
/// <b>Instantiation is the only way to create one</b>, which is why the pinned
/// version is required rather than nullable. Ad-hoc planning would need lightweight
/// objectives, not objectiveless plans.
/// </para>
/// </remarks>
public sealed class ProcessPlan : AggregateRoot<ProcessPlanId>
{
    public const int NameMaxLength = 300;

    private readonly List<ProcessStep> _steps = [];
    private readonly List<ProcessPlanStatusEntry> _history = [];

    // EF materialisation.
    private ProcessPlan()
    {
    }

    public TenantId TenantId { get; private set; } = default!;

    /// <summary>What this plan is for. Required, and immutable.</summary>
    public ProcessObjectiveId ProcessObjectiveId { get; private set; } = default!;

    /// <summary>
    /// The playbook version this was scheduled from — <b>pinned forever</b>
    /// (ADR-065 I4). Publishing a newer version changes nothing here.
    /// </summary>
    public ProcessDefinitionVersionId ProcessDefinitionVersionId
    { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    /// <summary>
    /// The date every schedule in this plan was derived from. Stored because it
    /// is half the answer to <em>"why is this milestone on this date?"</em> — the
    /// pinned version being the other half.
    /// </summary>
    public DateOnly AnchorDate { get; private set; }

    public ProcessPlanStatus CurrentStatus { get; private set; }

    /// <summary>Every step, in whatever order the database returned them.</summary>
    public IReadOnlyCollection<ProcessStep> Steps => _steps.AsReadOnly();

    public IReadOnlyList<ProcessPlanStatusEntry> History => _history.AsReadOnly();

    /// <summary>When the plan was drawn up — the first entry's business date.</summary>
    public DateOnly OpenedOn => _history[0].OccurredOn;

    /// <summary>
    /// Creates a plan by scheduling a published playbook version from an anchor
    /// date.
    /// </summary>
    /// <remarks>
    /// <b>Deterministic (ADR-065 I5).</b> The same version, objective and anchor
    /// always produce the same steps, the same dates and the same graph. Nothing
    /// here reads a clock, a database ordering, or any state outside its
    /// arguments — the steps are walked in code order, which is unique within a
    /// version, rather than in whatever order EF materialised them.
    /// <para>
    /// <b>It cashes publication's certificate rather than re-earning it.</b> A
    /// published version is a valid DAG suitable for instantiation
    /// (<c>ProcessDefinitionVersion.Publish</c>), so this method performs no cycle
    /// detection. Refusing an unpublished version is what makes that assumption
    /// sound; if the walk still fails, RegOS has broken its own guarantee and
    /// says so as an <see cref="InvalidOperationException"/>.
    /// </para>
    /// </remarks>
    public static ProcessPlan InstantiateFrom(
        TenantId tenantId,
        ProcessObjectiveId objectiveId,
        ProcessDefinitionVersion version,
        DateOnly anchorDate,
        string name,
        DateOnly openedOn)
    {
        if (tenantId is null)
            throw new DomainException(ProcessPlanErrors.TenantRequired);

        if (objectiveId is null)
            throw new DomainException(ProcessPlanErrors.ObjectiveRequired);

        if (version.Status != ProcessDefinitionVersionStatus.Published)
            throw new BusinessRuleViolationException(
                ProcessPlanErrors.VersionNotPublished);

        var plan = new ProcessPlan
        {
            Id = ProcessPlanId.New(),
            TenantId = tenantId,
            ProcessObjectiveId = objectiveId,
            ProcessDefinitionVersionId = version.Id,
            Name = ValidatedName(name),
            AnchorDate = anchorDate,
            CurrentStatus = ProcessPlanStatus.Draft
        };

        plan._history.Add(new ProcessPlanStatusEntry(
            ProcessPlanStatusEntryId.New(),
            ProcessPlanStatus.Draft,
            openedOn,
            DateTime.UtcNow,
            null));

        plan.Schedule(version, anchorDate, openedOn);

        return plan;
    }

    /// <summary>The team has committed to working this schedule.</summary>
    public void Activate(DateOnly occurredOn, string? note = null)
    {
        if (CurrentStatus == ProcessPlanStatus.Active)
            throw new BusinessRuleViolationException(ProcessPlanErrors.AlreadyActive);

        Move(ProcessPlanStatus.Active, occurredOn, note);
    }

    /// <summary>
    /// Nothing further is expected of this plan.
    /// </summary>
    /// <remarks>
    /// <b>It does not require every step to be complete</b> (ADR-065 D11). Steps
    /// may legitimately have been <see cref="ProcessStepStatus.Skipped"/>, and
    /// requiring otherwise would push a team to mark work done that was not, in
    /// order to close a plan. <b>The condition is a judgement, not a count</b> —
    /// exactly as a step's completion is.
    /// </remarks>
    public void Complete(DateOnly occurredOn, string? note = null)
        => Move(ProcessPlanStatus.Completed, occurredOn, note);

    /// <summary>Abandoned before finishing. The plan and its history are kept.</summary>
    public void Cancel(DateOnly occurredOn, string? note = null)
        => Move(ProcessPlanStatus.Cancelled, occurredOn, note);

    // --- execution (S004) ----------------------------------------------------

    /// <summary>Work on a step has begun.</summary>
    public void StartStep(ProcessStepId stepId, DateOnly occurredOn)
    {
        var step = StepOf(stepId);

        GuardStepCanMove(step, occurredOn);

        if (step.CurrentStatus == ProcessStepStatus.InProgress)
            throw new BusinessRuleViolationException(
                ProcessPlanErrors.StepAlreadyInProgress);

        step.RecordStatus(ProcessStepStatus.InProgress, occurredOn, null);
    }

    /// <summary>
    /// A person has decided this step is done, on a date they chose.
    /// </summary>
    /// <remarks>
    /// <b>Nothing else can call this on their behalf</b> (ADR-065 D11). A linked
    /// submission reaching <c>Transmitted</c>, a meeting recorded as held, or
    /// every predecessor completing may all <em>suggest</em> it. None of them
    /// performs it.
    /// <para>
    /// <b>A step may complete without ever having started.</b> Work finished that
    /// nobody marked as begun is ordinary, and refusing it would only teach people
    /// to record a fictional start date.
    /// </para>
    /// </remarks>
    public void CompleteStep(
        ProcessStepId stepId, DateOnly occurredOn, string? note = null)
    {
        var step = StepOf(stepId);

        GuardStepCanMove(step, occurredOn);
        GuardNote(note);

        step.RecordStatus(
            ProcessStepStatus.Complete,
            occurredOn,
            string.IsNullOrWhiteSpace(note) ? null : note.Trim());
    }

    /// <summary>
    /// This step will deliberately not be performed. <b>A reason is required.</b>
    /// </summary>
    /// <remarks>
    /// The one place this story adds friction on purpose. A skipped step with no
    /// reason is an unexplained gap in a regulatory record a year later, and
    /// <em>"Skipped"</em> on its own is not an explanation. Everywhere else in
    /// RegOS a note is optional; here it is the record of why the work was not
    /// done.
    /// </remarks>
    public void SkipStep(ProcessStepId stepId, DateOnly occurredOn, string reason)
    {
        var step = StepOf(stepId);

        GuardStepCanMove(step, occurredOn);

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException(ProcessPlanErrors.SkipReasonRequired);

        GuardNote(reason);

        step.RecordStatus(ProcessStepStatus.Skipped, occurredOn, reason.Trim());
    }

    /// <summary>
    /// Moves a planned date. <b>Permitted, and it moves nothing else</b> — a
    /// schedule is a statement of intent a human owns (D5), and only execution is
    /// history (I6).
    /// </summary>
    public void RescheduleStep(
        ProcessStepId stepId, DateOnly plannedStartOn, DateOnly plannedEndOn)
        => StepOf(stepId).Reschedule(plannedStartOn, plannedEndOn);

    private ProcessStep StepOf(ProcessStepId stepId)
        => _steps.FirstOrDefault(x => x.Id == stepId)
           ?? throw new NotFoundException(ProcessPlanErrors.StepNotFound);

    private void GuardStepCanMove(ProcessStep step, DateOnly occurredOn)
    {
        if (CurrentStatus != ProcessPlanStatus.Active)
            throw new BusinessRuleViolationException(ProcessPlanErrors.NotActive);

        if (step.IsSettled)
            throw new BusinessRuleViolationException(
                ProcessPlanErrors.StepAlreadySettled);

        // Max, not [^1]: an unordered Include returns the last row the database
        // gave, not the latest event.
        if (occurredOn < step.LatestRecordedOn)
            throw new DomainException(ProcessPlanErrors.StepHistoryOutOfOrder);
    }

    private static void GuardNote(string? note)
    {
        if (note is { Length: > ProcessStepStatusEntry.NoteMaxLength })
            throw new DomainException(ProcessPlanErrors.NoteTooLong);
    }

    private void Move(ProcessPlanStatus target, DateOnly occurredOn, string? note)
    {
        if (CurrentStatus is ProcessPlanStatus.Completed
            or ProcessPlanStatus.Cancelled)
            throw new BusinessRuleViolationException(ProcessPlanErrors.AlreadyClosed);

        if (occurredOn < _history.Max(entry => entry.OccurredOn))
            throw new DomainException(ProcessPlanErrors.HistoryOutOfOrder);

        if (note is { Length: > ProcessPlanStatusEntry.NoteMaxLength })
            throw new DomainException(ProcessPlanErrors.NoteTooLong);

        _history.Add(new ProcessPlanStatusEntry(
            ProcessPlanStatusEntryId.New(),
            target,
            occurredOn,
            DateTime.UtcNow,
            string.IsNullOrWhiteSpace(note) ? null : note.Trim()));

        CurrentStatus = target;
    }

    /// <summary>
    /// The derivation. Walks the definition's steps in dependency order and
    /// writes each one's dates <b>once</b>.
    /// </summary>
    /// <remarks>
    /// <code>
    /// no predecessors:  start = anchor + OffsetDays
    /// otherwise:        start = max(predecessor.PlannedEndOn) + 1 + OffsetDays
    ///                   end   = start + DurationDays - 1
    /// </code>
    /// Dates are <b>inclusive</b>, and <c>OffsetDays = 0</c> means <em>"the day
    /// after the last thing it waits for finishes"</em>.
    /// </remarks>
    private void Schedule(
        ProcessDefinitionVersion version, DateOnly anchorDate, DateOnly openedOn)
    {
        // Ordered by code, not by the collection's order: EF materialises an
        // Include in whatever order the database returned, and I5 forbids that
        // influencing the result. Code is unique within a version.
        var definitions = version.Steps
            .OrderBy(step => step.Code, StringComparer.Ordinal)
            .ToList();

        var outstanding = definitions.ToDictionary(step => step.Id);
        var schedule = new Dictionary<ProcessStepDefinitionId, (DateOnly Start, DateOnly End)>();

        while (outstanding.Count > 0)
        {
            var ready = definitions
                .Where(step => outstanding.ContainsKey(step.Id))
                .Where(step => step.Predecessors.All(
                    predecessor => schedule.ContainsKey(predecessor.PredecessorStepId)))
                .ToList();

            if (ready.Count == 0)
            {
                // Publication certified this cannot happen. It has, so the
                // guarantee is broken rather than the request.
                throw new InvalidOperationException(
                    ProcessPlanErrors.CertificateBroken
                    + string.Join(", ", outstanding.Values.Select(step => step.Code)));
            }

            foreach (var step in ready)
            {
                var start = step.Predecessors.Count == 0
                    ? anchorDate.AddDays(step.OffsetDays)
                    : step.Predecessors
                        .Max(predecessor => schedule[predecessor.PredecessorStepId].End)
                        .AddDays(1 + step.OffsetDays);

                schedule[step.Id] = (start, start.AddDays(step.DurationDays - 1));

                outstanding.Remove(step.Id);
            }
        }

        // Live ids for authored ids, so the plan's graph is its own.
        var liveIds = definitions.ToDictionary(
            step => step.Id, _ => ProcessStepId.New());

        foreach (var definition in definitions)
        {
            var (start, end) = schedule[definition.Id];

            _steps.Add(new ProcessStep(
                liveIds[definition.Id],
                definition.Id,
                definition.Code,
                definition.Name,
                definition.Description,
                definition.ParentStepId is { } parent ? liveIds[parent] : null,
                definition.Order,
                start,
                end,
                openedOn));
        }

        var stepsById = _steps.ToDictionary(step => step.StepDefinitionId);

        foreach (var definition in definitions)
        {
            foreach (var predecessor in definition.Predecessors
                         // Deterministic: a step waits for another at most once
                         // (unique index), so ordering by the authored id is total.
                         .OrderBy(x => x.PredecessorStepId.Value))
            {
                stepsById[definition.Id].WaitFor(liveIds[predecessor.PredecessorStepId]);
            }
        }
    }

    private static string ValidatedName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ProcessPlanErrors.NameRequired);

        var trimmed = name.Trim();

        return trimmed.Length > NameMaxLength
            ? throw new DomainException(ProcessPlanErrors.NameTooLong)
            : trimmed;
    }
}
