using RegOS.Labeling.Domain.Aggregates.GlobalLabels;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.StartGlobalLabelDraft;

public sealed class StartGlobalLabelDraftHandler
{
    private readonly IGlobalLabelRepository _labels;

    public StartGlobalLabelDraftHandler(IGlobalLabelRepository labels)
    {
        _labels = labels;
    }

    public async Task<StartGlobalLabelDraftResult> HandleAsync(
        StartGlobalLabelDraftCommand command,
        CancellationToken cancellationToken)
    {
        var label = await _labels.GetByIdAsync(
                command.GlobalLabelId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.GlobalLabelDoesNotExist);

        var draft = label.StartDraft();

        await _labels.UpdateAsync(label, cancellationToken);

        return new StartGlobalLabelDraftResult(draft.Id, draft.VersionNumber);
    }
}
