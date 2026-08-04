using RegOS.Labeling.Domain.Aggregates.Indications;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.RemoveIndicationPopulation;

public sealed class RemoveIndicationPopulationHandler
{
    private readonly IIndicationRepository _indications;

    public RemoveIndicationPopulationHandler(IIndicationRepository indications)
    {
        _indications = indications;
    }

    public async Task HandleAsync(
        RemoveIndicationPopulationCommand command,
        CancellationToken cancellationToken)
    {
        var indication = await _indications.GetByIdAsync(
                command.IndicationId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.IndicationDoesNotExist);

        indication.RemovePopulation(command.PopulationId);

        await _indications.UpdateAsync(indication, cancellationToken);
    }
}
