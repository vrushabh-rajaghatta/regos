using RegOS.Labeling.Domain.Aggregates.Indications;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.AddIndicationPopulation;

public sealed class AddIndicationPopulationHandler
{
    private readonly IIndicationRepository _indications;

    public AddIndicationPopulationHandler(IIndicationRepository indications)
    {
        _indications = indications;
    }

    public async Task HandleAsync(
        AddIndicationPopulationCommand command,
        CancellationToken cancellationToken)
    {
        var indication = await _indications.GetByIdAsync(
                command.IndicationId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.IndicationDoesNotExist);

        indication.AddPopulation(
            command.AgeLow,
            command.AgeHigh,
            command.AgeUnitCode,
            command.GenderCode,
            command.PhysiologicalConditionCode,
            command.Description);

        await _indications.UpdateAsync(indication, cancellationToken);
    }
}
