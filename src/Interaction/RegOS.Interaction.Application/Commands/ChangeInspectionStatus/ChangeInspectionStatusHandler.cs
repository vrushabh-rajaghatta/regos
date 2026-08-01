using RegOS.Interaction.Domain.Inspections;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Interaction.Application.Commands.ChangeInspectionStatus;

public sealed class ChangeInspectionStatusHandler
{
    private readonly IInspectionRepository _repository;

    public ChangeInspectionStatusHandler(IInspectionRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        ChangeInspectionStatusCommand command,
        CancellationToken cancellationToken)
    {
        var inspection =
            await _repository.GetByIdAsync(command.InspectionId, cancellationToken)
            ?? throw new NotFoundException("The inspection was not found.");

        inspection.ChangeStatus(command.Target, command.OccurredOn, command.Note);

        await _repository.UpdateAsync(inspection, cancellationToken);
    }
}
