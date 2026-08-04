using RegOS.Labeling.Domain.Aggregates.LocalLabels;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.DiscardLocalLabelDraft;

public sealed class DiscardLocalLabelDraftHandler
{
    private readonly ILocalLabelRepository _labels;

    public DiscardLocalLabelDraftHandler(ILocalLabelRepository labels)
    {
        _labels = labels;
    }

    public async Task HandleAsync(
        DiscardLocalLabelDraftCommand command,
        CancellationToken cancellationToken)
    {
        var label = await _labels.GetByIdAsync(
                command.LocalLabelId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.LocalLabelDoesNotExist);

        label.DiscardDraft();

        await _labels.UpdateAsync(label, cancellationToken);
    }
}
