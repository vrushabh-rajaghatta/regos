using RegOS.Labeling.Domain.Aggregates.Indications;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.RemoveIndicationTherapy;

public sealed class RemoveIndicationTherapyHandler
{
    private readonly IIndicationRepository _indications;

    public RemoveIndicationTherapyHandler(IIndicationRepository indications)
    {
        _indications = indications;
    }

    public async Task HandleAsync(
        RemoveIndicationTherapyCommand command,
        CancellationToken cancellationToken)
    {
        var indication = await _indications.GetByIdAsync(
                command.IndicationId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.IndicationDoesNotExist);

        indication.RemoveOtherTherapy(command.OtherTherapyId);

        await _indications.UpdateAsync(indication, cancellationToken);
    }
}
