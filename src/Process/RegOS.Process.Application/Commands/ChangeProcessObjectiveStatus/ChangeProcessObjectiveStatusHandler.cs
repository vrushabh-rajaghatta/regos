using RegOS.Process.Domain.Aggregates.ProcessObjectives;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Process.Application.Commands.ChangeProcessObjectiveStatus;

/// <summary>
/// Moves an objective through its lifecycle of <em>intent</em>. Every rule —
/// terminal states, no return to proposed, chronology — belongs to the
/// aggregate; this handler only loads and saves.
/// </summary>
public sealed class ChangeProcessObjectiveStatusHandler
{
    private readonly IProcessObjectiveRepository _objectives;

    public ChangeProcessObjectiveStatusHandler(
        IProcessObjectiveRepository objectives)
    {
        _objectives = objectives;
    }

    public async Task HandleAsync(
        ChangeProcessObjectiveStatusCommand command,
        CancellationToken cancellationToken)
    {
        var objective =
            await _objectives.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("That objective does not exist.");

        objective.ChangeStatus(command.Status, command.OccurredOn, command.Note);

        await _objectives.UpdateAsync(objective, cancellationToken);
    }
}
