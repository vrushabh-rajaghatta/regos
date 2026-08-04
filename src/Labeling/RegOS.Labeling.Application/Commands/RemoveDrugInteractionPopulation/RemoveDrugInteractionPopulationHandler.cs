using RegOS.Labeling.Domain.Aggregates.DrugInteractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.RemoveDrugInteractionPopulation;

public sealed class RemoveDrugInteractionPopulationHandler
{
    private readonly IDrugInteractionRepository _interactions;

    public RemoveDrugInteractionPopulationHandler(IDrugInteractionRepository interactions)
    {
        _interactions = interactions;
    }

    public async Task HandleAsync(
        RemoveDrugInteractionPopulationCommand command,
        CancellationToken cancellationToken)
    {
        var interaction = await _interactions.GetByIdAsync(
                command.DrugInteractionId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.ClinicalStatementDoesNotExist);

        interaction.RemovePopulation(command.PopulationId);

        await _interactions.UpdateAsync(interaction, cancellationToken);
    }
}
