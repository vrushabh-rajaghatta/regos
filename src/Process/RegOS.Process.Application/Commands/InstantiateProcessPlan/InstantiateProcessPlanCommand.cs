using RegOS.Process.Domain.Aggregates.ProcessDefinitions;
using RegOS.Process.Domain.Aggregates.ProcessObjectives;

namespace RegOS.Process.Application.Commands.InstantiateProcessPlan;

/// <summary>
/// Schedules a published playbook version against an objective, from an anchor
/// date.
/// </summary>
/// <param name="ProcessDefinitionVersionId">
/// A <em>version</em>, never a playbook. Resolving "the current one" here would
/// make the plan's schedule depend on when it was created rather than on what it
/// was created from (ADR-065 I5).
/// </param>
public sealed record InstantiateProcessPlanCommand(
    ProcessObjectiveId ProcessObjectiveId,
    ProcessDefinitionVersionId ProcessDefinitionVersionId,
    DateOnly AnchorDate,
    string Name,
    DateOnly OpenedOn);
