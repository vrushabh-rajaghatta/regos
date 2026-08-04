using RegOS.Labeling.Domain.Aggregates.DrugInteractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.AddInteractant;

public sealed class AddInteractantHandler
{
    private readonly IDrugInteractionRepository _interactions;

    public AddInteractantHandler(IDrugInteractionRepository interactions)
    {
        _interactions = interactions;
    }

    public async Task HandleAsync(
        AddInteractantCommand command,
        CancellationToken cancellationToken)
    {
        var interaction = await _interactions.GetByIdAsync(
                command.DrugInteractionId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.ClinicalStatementDoesNotExist);

        interaction.AddInteractant(
            command.Description, command.SubstanceId);

        await _interactions.UpdateAsync(interaction, cancellationToken);
    }
}
