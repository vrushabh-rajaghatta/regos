using RegOS.Labeling.Domain.Aggregates.GlobalLabels;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.DiscardGlobalLabelDraft;

public sealed class DiscardGlobalLabelDraftHandler
{
    private readonly IGlobalLabelRepository _labels;

    public DiscardGlobalLabelDraftHandler(IGlobalLabelRepository labels)
    {
        _labels = labels;
    }

    public async Task HandleAsync(
        DiscardGlobalLabelDraftCommand command,
        CancellationToken cancellationToken)
    {
        var label = await _labels.GetByIdAsync(
                command.GlobalLabelId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.GlobalLabelDoesNotExist);

        label.DiscardDraft();

        await _labels.UpdateAsync(label, cancellationToken);
    }
}
