using RegOS.Labeling.Domain.Aggregates.DrugInteractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.AddDrugInteractionPopulation;

public sealed class AddDrugInteractionPopulationHandler
{
    private readonly IDrugInteractionRepository _interactions;

    public AddDrugInteractionPopulationHandler(IDrugInteractionRepository interactions)
    {
        _interactions = interactions;
    }

    public async Task HandleAsync(
        AddDrugInteractionPopulationCommand command,
        CancellationToken cancellationToken)
    {
        var interaction = await _interactions.GetByIdAsync(
                command.DrugInteractionId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.ClinicalStatementDoesNotExist);

        interaction.AddPopulation(
            command.AgeLow,
            command.AgeHigh,
            command.AgeUnitCode,
            command.GenderCode,
            command.PhysiologicalConditionCode,
            command.Description);

        await _interactions.UpdateAsync(interaction, cancellationToken);
    }
}
