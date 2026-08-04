using RegOS.Labeling.Domain.Aggregates.DrugInteractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.RemoveInteractant;

public sealed class RemoveInteractantHandler
{
    private readonly IDrugInteractionRepository _interactions;

    public RemoveInteractantHandler(IDrugInteractionRepository interactions)
    {
        _interactions = interactions;
    }

    public async Task HandleAsync(
        RemoveInteractantCommand command,
        CancellationToken cancellationToken)
    {
        var interaction = await _interactions.GetByIdAsync(
                command.DrugInteractionId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.ClinicalStatementDoesNotExist);

        interaction.RemoveInteractant(command.InteractantId);

        await _interactions.UpdateAsync(interaction, cancellationToken);
    }
}
