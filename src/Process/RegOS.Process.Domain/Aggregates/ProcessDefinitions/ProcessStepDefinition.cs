using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Process.Domain.Aggregates.ProcessDefinitions;

/// <summary>
/// One step of a playbook: what has to be done, what it waits for, and how long
/// it takes.
/// </summary>
/// <remarks>
/// <b>A description of work, not the work.</b> Nothing here has a date. Dates
/// arrive only when a plan is instantiated from the version this step belongs to,
/// and they are written once
/// ([ADR-065](../../../../../docs/adr/ADR-065-regulatory-process-is-an-optional-bounded-context.md)
/// decision 4). This type holds the two numbers that make that derivation
/// possible and stores no schedule of its own.
/// <para>
/// <b><see cref="OffsetDays"/> is relative, never absolute.</b> A step with
/// predecessors starts that many days after the last of them finishes; a step
/// with none starts that many days after the plan's anchor date. Storing an
/// absolute date here would make the playbook expire.
/// </para>
/// </remarks>
public sealed class ProcessStepDefinition : Entity<ProcessStepDefinitionId>
{
    public const int CodeMaxLength = 100;
    public const int NameMaxLength = 300;
    public const int DescriptionMaxLength = 4000;

    private readonly List<ProcessStepPredecessor> _predecessors = [];

    // EF materialisation.
    private ProcessStepDefinition()
    {
    }

    internal ProcessStepDefinition(
        ProcessStepDefinitionId id,
        string code,
        string name,
        string? description,
        ProcessStepDefinitionId? parentStepId,
        int order,
        int offsetDays,
        int durationDays)
    {
        Id = id;
        Code = Validated(
            code,
            CodeMaxLength,
            ProcessDefinitionErrors.StepCodeRequired,
            ProcessDefinitionErrors.StepCodeTooLong).ToUpperInvariant();
        Name = Validated(
            name,
            NameMaxLength,
            ProcessDefinitionErrors.StepNameRequired,
            ProcessDefinitionErrors.StepNameTooLong);
        Description = OptionalDescription(description);
        ParentStepId = parentStepId;
        Order = order;
        OffsetDays = ValidatedOffset(offsetDays);
        DurationDays = ValidatedDuration(durationDays);
    }

    /// <summary>Stable within a version — how a seed or an import names the step.</summary>
    public string Code { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    public string? Description { get; private set; }

    /// <summary>
    /// The step this one is a part of, when the playbook groups its work into
    /// phases. Structural only — a parent imposes no ordering, which is what
    /// predecessors are for.
    /// </summary>
    public ProcessStepDefinitionId? ParentStepId { get; private set; }

    /// <summary>
    /// Display order among siblings. <b>Not unique, and not a schedule</b> — two
    /// steps may legitimately share an order, so every read that sorts by it ends
    /// in a unique key as well.
    /// </summary>
    public int Order { get; private set; }

    /// <summary>Days after the last predecessor finishes — or after the plan's anchor.</summary>
    public int OffsetDays { get; private set; }

    /// <summary>How long the step is expected to take, in days. At least one.</summary>
    public int DurationDays { get; private set; }

    /// <summary>What this step waits for. Empty means it waits for the anchor.</summary>
    public IReadOnlyCollection<ProcessStepPredecessor> Predecessors
        => _predecessors.AsReadOnly();

    internal void AddPredecessor(ProcessStepDefinitionId predecessorStepId)
    {
        if (predecessorStepId == Id)
            throw new BusinessRuleViolationException(
                ProcessDefinitionErrors.StepCannotPrecedeItself);

        if (_predecessors.Any(x => x.PredecessorStepId == predecessorStepId))
            throw new BusinessRuleViolationException(
                ProcessDefinitionErrors.DuplicatePredecessor);

        _predecessors.Add(new ProcessStepPredecessor(predecessorStepId));
    }

    private static string Validated(
        string value, int maxLength, string required, string tooLong)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(required);

        var trimmed = value.Trim();

        if (trimmed.Length > maxLength)
            throw new DomainException(tooLong);

        return trimmed;
    }

    private static string? OptionalDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;

        var trimmed = description.Trim();

        return trimmed.Length > DescriptionMaxLength
            ? throw new DomainException(
                ProcessDefinitionErrors.StepDescriptionTooLong)
            : trimmed;
    }

    private static int ValidatedOffset(int offsetDays)
        => offsetDays < 0
            ? throw new DomainException(ProcessDefinitionErrors.OffsetDaysNegative)
            : offsetDays;

    private static int ValidatedDuration(int durationDays)
        => durationDays < 1
            ? throw new DomainException(ProcessDefinitionErrors.DurationDaysNotPositive)
            : durationDays;
}
