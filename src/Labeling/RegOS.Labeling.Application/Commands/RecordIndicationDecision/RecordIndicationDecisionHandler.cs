using RegOS.Labeling.Domain.Aggregates.Indications;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.RecordIndicationDecision;

public sealed class RecordIndicationDecisionHandler
{
    private readonly IIndicationRepository _indications;

    public RecordIndicationDecisionHandler(IIndicationRepository indications)
    {
        _indications = indications;
    }

    public async Task HandleAsync(
        RecordIndicationDecisionCommand command,
        CancellationToken cancellationToken)
    {
        var indication = await _indications.GetByIdAsync(
                command.IndicationId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.IndicationDoesNotExist);

        indication.RecordDecision(
            command.Status, command.OccurredOn, command.Note);

        await _indications.UpdateAsync(indication, cancellationToken);
    }
}
