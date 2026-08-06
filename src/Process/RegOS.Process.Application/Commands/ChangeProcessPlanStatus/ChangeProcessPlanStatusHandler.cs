using RegOS.Process.Domain.Aggregates.ProcessPlans;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Process.Application.Commands.ChangeProcessPlanStatus;

/// <summary>
/// Activates, completes or cancels a plan. Every rule belongs to the aggregate;
/// this loads and saves.
/// </summary>
public sealed class ChangeProcessPlanStatusHandler
{
    private readonly IProcessPlanRepository _plans;

    public ChangeProcessPlanStatusHandler(IProcessPlanRepository plans)
    {
        _plans = plans;
    }

    public async Task HandleAsync(
        ChangeProcessPlanStatusCommand command,
        CancellationToken cancellationToken)
    {
        var plan = await _plans.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("That plan does not exist.");

        switch (command.Status)
        {
            case ProcessPlanStatus.Active:
                plan.Activate(command.OccurredOn, command.Note);
                break;

            case ProcessPlanStatus.Completed:
                plan.Complete(command.OccurredOn, command.Note);
                break;

            case ProcessPlanStatus.Cancelled:
                plan.Cancel(command.OccurredOn, command.Note);
                break;

            // Draft is where a plan starts and is not a destination — there is
            // no path back to it, which is the aggregate's rule restated here
            // only because a switch has to be total.
            default:
                throw new BusinessRuleViolationException(
                    ProcessPlanErrors.AlreadyClosed);
        }

        await _plans.UpdateAsync(plan, cancellationToken);
    }
}
