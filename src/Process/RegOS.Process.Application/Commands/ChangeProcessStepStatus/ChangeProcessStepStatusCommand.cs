using RegOS.Process.Domain.Aggregates.ProcessPlans;

namespace RegOS.Process.Application.Commands.ChangeProcessStepStatus;

/// <param name="Note">
/// Optional for progress and completion; <b>required</b> when skipping, where it
/// becomes the record of why the work was not done (ADR-065 D11).
/// </param>
public sealed record ChangeProcessStepStatusCommand(
    ProcessPlanId PlanId,
    ProcessStepId StepId,
    ProcessStepStatus Status,
    DateOnly OccurredOn,
    string? Note = null);
