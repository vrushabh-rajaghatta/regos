using RegOS.Labeling.Domain.Aggregates.LocalLabels;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.PublishLocalLabelRevision;

public sealed class PublishLocalLabelRevisionHandler
{
    private readonly ILocalLabelRepository _labels;

    public PublishLocalLabelRevisionHandler(ILocalLabelRepository labels)
    {
        _labels = labels;
    }

    public async Task HandleAsync(
        PublishLocalLabelRevisionCommand command,
        CancellationToken cancellationToken)
    {
        var label = await _labels.GetByIdAsync(
                command.LocalLabelId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.LocalLabelDoesNotExist);

        label.PublishRevision(
            command.RevisionId,
            command.ApprovedOn,
            command.EffectiveFrom);

        await _labels.UpdateAsync(label, cancellationToken);
    }
}
