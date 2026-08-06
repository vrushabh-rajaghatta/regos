using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Process.Domain.Aggregates.ProcessDefinitions;

/// <summary>
/// One version of a playbook — <b>the governance artefact a plan is pinned to.</b>
/// </summary>
/// <remarks>
/// <b>This is the type
/// [ADR-065](../../../../../docs/adr/ADR-065-regulatory-process-is-an-optional-bounded-context.md)
/// I4 is about.</b> Published is a one-way door: once a version is published its
/// steps can never change, because a plan may already have been scheduled from
/// it and *"why did this milestone move?"* must always have an answer. Superseding
/// it says only that nothing new should instantiate from it.
/// <para>
/// <b>A plan pins this, not <see cref="ProcessDefinition"/></b> — the same
/// deliberate exception [ADR-035 §2](../../../../../docs/adr/ADR-035-submissions-bind-to-a-published-template-version.md)
/// makes for submissions. Referencing the root would leave *"which version?"*
/// unanswered at every point that matters.
/// </para>
/// <para>
/// <b>The cycle check lives at <see cref="Publish"/>, not at instantiation</b>
/// (ADR-065 decision 4). A circle of predecessors makes every start date
/// underivable, and the playbook — not the plan somebody tried to make from it —
/// is the thing that is wrong. Catching it here means a published version is
/// always schedulable.
/// </para>
/// </remarks>
public sealed class ProcessDefinitionVersion : Entity<ProcessDefinitionVersionId>
{
    private readonly List<ProcessStepDefinition> _steps = [];

    // EF materialisation.
    private ProcessDefinitionVersion()
    {
    }

    // Internal: only ProcessDefinition may open a version, so there is no path
    // to one that does not belong to a playbook.
    internal ProcessDefinitionVersion(
        ProcessDefinitionVersionId id,
        int versionNumber)
    {
        Id = id;
        VersionNumber = versionNumber;
        Status = ProcessDefinitionVersionStatus.Draft;
    }

    public int VersionNumber { get; private set; }

    public ProcessDefinitionVersionStatus Status { get; private set; }

    /// <summary>
    /// When this version became the one to instantiate from. Set at publish.
    /// </summary>
    /// <remarks>
    /// There is no <c>EffectiveTo</c>: a superseded version's end is the next
    /// one's start, and a second stored copy of that date could disagree with it.
    /// </remarks>
    public DateOnly? EffectiveFrom { get; private set; }

    public DateTime? PublishedOnUtc { get; private set; }

    public IReadOnlyCollection<ProcessStepDefinition> Steps => _steps.AsReadOnly();

    /// <summary>
    /// Freezes this version. <b>Publication certifies that the definition is a
    /// valid DAG suitable for plan instantiation</b> — so instantiation never
    /// validates the graph, because publication already did.
    /// </summary>
    /// <remarks>
    /// That is a stronger guarantee than <em>"cycles are not allowed"</em>, and it
    /// is where the expensive check belongs: once, when a steward publishes,
    /// rather than on every plan created from it forever after.
    /// </remarks>
    internal void Publish(DateOnly? effectiveFrom, DateTime publishedOnUtc)
    {
        if (Status == ProcessDefinitionVersionStatus.Published)
            throw new BusinessRuleViolationException(
                ProcessDefinitionErrors.VersionAlreadyPublished);

        if (Status != ProcessDefinitionVersionStatus.Draft)
            throw new BusinessRuleViolationException(
                ProcessDefinitionErrors.VersionNotDraft);

        if (_steps.Count == 0)
            throw new BusinessRuleViolationException(
                ProcessDefinitionErrors.NoSteps);

        GuardTheStepsCanBeScheduled();

        Status = ProcessDefinitionVersionStatus.Published;
        EffectiveFrom = effectiveFrom;
        PublishedOnUtc = publishedOnUtc;
    }

    internal void Supersede()
    {
        if (Status == ProcessDefinitionVersionStatus.Superseded)
            throw new BusinessRuleViolationException(
                ProcessDefinitionErrors.VersionAlreadySuperseded);

        // A draft nobody could have used is discarded, not superseded.
        // Superseding is a statement about something that was in force.
        if (Status != ProcessDefinitionVersionStatus.Published)
            throw new BusinessRuleViolationException(
                ProcessDefinitionErrors.OnlyPublishedVersionsCanBeSuperseded);

        Status = ProcessDefinitionVersionStatus.Superseded;
    }

    internal ProcessStepDefinition AddStep(
        string code,
        string name,
        string? description,
        ProcessStepDefinitionId? parentStepId,
        int order,
        int offsetDays,
        int durationDays)
    {
        GuardIsDraft();

        // Construct first — the step validates its own code, name and numbers.
        var step = new ProcessStepDefinition(
            ProcessStepDefinitionId.New(),
            code,
            name,
            description,
            parentStepId,
            order,
            offsetDays,
            durationDays);

        if (_steps.Any(x => string.Equals(
                x.Code, step.Code, StringComparison.OrdinalIgnoreCase)))
            throw new BusinessRuleViolationException(
                ProcessDefinitionErrors.DuplicateStepCode);

        if (step.ParentStepId is { } parentId && _steps.All(x => x.Id != parentId))
            throw new BusinessRuleViolationException(
                ProcessDefinitionErrors.ParentStepNotFound);

        _steps.Add(step);

        return step;
    }

    internal void AddPredecessor(
        ProcessStepDefinitionId stepId,
        ProcessStepDefinitionId predecessorStepId)
    {
        GuardIsDraft();

        var step = StepOf(stepId);

        if (_steps.All(x => x.Id != predecessorStepId))
            throw new BusinessRuleViolationException(
                ProcessDefinitionErrors.PredecessorNotFound);

        step.AddPredecessor(predecessorStepId);
    }

    private ProcessStepDefinition StepOf(ProcessStepDefinitionId stepId)
        => _steps.FirstOrDefault(x => x.Id == stepId)
           ?? throw new NotFoundException(ProcessDefinitionErrors.StepNotFound);

    private void GuardIsDraft()
    {
        if (Status != ProcessDefinitionVersionStatus.Draft)
            throw new BusinessRuleViolationException(
                ProcessDefinitionErrors.VersionNotDraft);
    }

    /// <summary>
    /// Refuses to publish a step graph no anchor date could resolve.
    /// </summary>
    /// <remarks>
    /// Kahn's algorithm, and the interesting part is the verdict rather than the
    /// walk: if any step is still unresolved when nothing more can be scheduled,
    /// those steps are waiting on each other. Predecessors only ever point
    /// backwards, so a cycle is the single way this can fail.
    /// </remarks>
    private void GuardTheStepsCanBeScheduled()
    {
        var outstanding = _steps.ToDictionary(
            step => step.Id,
            step => step.Predecessors
                .Select(x => x.PredecessorStepId)
                .ToHashSet());

        var scheduled = new HashSet<ProcessStepDefinitionId>();

        while (true)
        {
            // Everything whose predecessors have all been scheduled already.
            var ready = outstanding
                .Where(entry => entry.Value.All(scheduled.Contains))
                .Select(entry => entry.Key)
                .ToList();

            if (ready.Count == 0)
                break;

            foreach (var id in ready)
            {
                scheduled.Add(id);
                outstanding.Remove(id);
            }
        }

        if (outstanding.Count > 0)
            throw new BusinessRuleViolationException(
                ProcessDefinitionErrors.PredecessorCycle);
    }
}
