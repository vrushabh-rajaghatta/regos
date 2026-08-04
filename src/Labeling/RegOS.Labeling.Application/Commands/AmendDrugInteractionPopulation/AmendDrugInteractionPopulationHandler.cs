using RegOS.Labeling.Domain.Aggregates.DrugInteractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.AmendDrugInteractionPopulation;

public sealed class AmendDrugInteractionPopulationHandler
{
    private readonly IDrugInteractionRepository _interactions;

    public AmendDrugInteractionPopulationHandler(IDrugInteractionRepository interactions)
    {
        _interactions = interactions;
    }

    public async Task HandleAsync(
        AmendDrugInteractionPopulationCommand command,
        CancellationToken cancellationToken)
    {
        var interaction = await _interactions.GetByIdAsync(
                command.DrugInteractionId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.ClinicalStatementDoesNotExist);

        interaction.AmendPopulation(
            command.PopulationId,
            command.AgeLow,
            command.AgeHigh,
            command.AgeUnitCode,
            command.GenderCode,
            command.PhysiologicalConditionCode,
            command.Description);

        await _interactions.UpdateAsync(interaction, cancellationToken);
    }
}
