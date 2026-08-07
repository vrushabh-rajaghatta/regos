using RegOS.Process.Domain.Aggregates.ProcessPlans;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Process.Application.Commands.ChangeProcessStepStatus;

/// <summary>
/// Records that a person decided a step started, finished, or will not be done.
/// </summary>
/// <remarks>
/// <b>Nothing but a user reaches this</b> (ADR-065 D11). There is deliberately no
/// path from a submission's lifecycle, a meeting being recorded, or a
/// predecessor completing — those may inform the decision and never perform it.
/// </remarks>
public sealed class ChangeProcessStepStatusHandler
{
    private readonly IProcessPlanRepository _plans;

    public ChangeProcessStepStatusHandler(IProcessPlanRepository plans)
    {
        _plans = plans;
    }

    public async Task HandleAsync(
        ChangeProcessStepStatusCommand command,
        CancellationToken cancellationToken)
    {
        var plan = await _plans.GetByIdAsync(command.PlanId, cancellationToken)
            ?? throw new NotFoundException("That plan does not exist.");

        switch (command.Status)
        {
            case ProcessStepStatus.InProgress:
                plan.StartStep(command.StepId, command.OccurredOn);
                break;

            case ProcessStepStatus.Complete:
                plan.CompleteStep(command.StepId, command.OccurredOn, command.Note);
                break;

            case ProcessStepStatus.Skipped:
                plan.SkipStep(
                    command.StepId, command.OccurredOn, command.Note ?? string.Empty);
                break;

            // NotStarted is where a step begins, and I6 makes it unreachable
            // afterwards: going back is a new fact, not an erasure of an old one.
            default:
                throw new BusinessRuleViolationException(
                    ProcessPlanErrors.StepAlreadySettled);
        }

        await _plans.UpdateAsync(plan, cancellationToken);
    }
}
