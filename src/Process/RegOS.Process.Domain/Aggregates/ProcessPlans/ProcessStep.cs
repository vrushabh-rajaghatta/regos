using RegOS.Process.Domain.Aggregates.ProcessDefinitions;
using RegOS.SharedKernel.Abstractions;

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
        DateOnly plannedEndOn)
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

    internal void WaitFor(ProcessStepId predecessorStepId)
        => _predecessors.Add(new ProcessStepDependency(predecessorStepId));
}
