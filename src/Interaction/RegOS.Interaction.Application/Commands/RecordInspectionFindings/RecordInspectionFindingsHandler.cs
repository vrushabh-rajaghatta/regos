using RegOS.Interaction.Domain.Inspections;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Interaction.Application.Commands.RecordInspectionFindings;

public sealed class RecordInspectionFindingsHandler
{
    private readonly IInspectionRepository _repository;

    public RecordInspectionFindingsHandler(IInspectionRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        RecordInspectionFindingsCommand command,
        CancellationToken cancellationToken)
    {
        var inspection =
            await _repository.GetByIdAsync(command.InspectionId, cancellationToken)
            ?? throw new NotFoundException("The inspection was not found.");

        inspection.RecordFindings(command.Findings);

        await _repository.UpdateAsync(inspection, cancellationToken);
    }
}
