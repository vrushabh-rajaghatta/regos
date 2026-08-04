using RegOS.Labeling.Domain.Aggregates.Indications;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.RestateIndicationText;

public sealed class RestateIndicationTextHandler
{
    private readonly IIndicationRepository _indications;

    public RestateIndicationTextHandler(IIndicationRepository indications)
    {
        _indications = indications;
    }

    public async Task HandleAsync(
        RestateIndicationTextCommand command,
        CancellationToken cancellationToken)
    {
        var indication = await _indications.GetByIdAsync(
                command.IndicationId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.IndicationDoesNotExist);

        indication.RestateLabelText(command.LabelText);

        await _indications.UpdateAsync(indication, cancellationToken);
    }
}
