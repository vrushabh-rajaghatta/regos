using RegOS.Labeling.Domain.Aggregates.Indications;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.AddIndicationTherapy;

public sealed class AddIndicationTherapyHandler
{
    private readonly IIndicationRepository _indications;

    public AddIndicationTherapyHandler(IIndicationRepository indications)
    {
        _indications = indications;
    }

    public async Task HandleAsync(
        AddIndicationTherapyCommand command,
        CancellationToken cancellationToken)
    {
        var indication = await _indications.GetByIdAsync(
                command.IndicationId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.IndicationDoesNotExist);

        indication.AddOtherTherapy(
            command.RelationshipCode, command.Therapy);

        await _indications.UpdateAsync(indication, cancellationToken);
    }
}
