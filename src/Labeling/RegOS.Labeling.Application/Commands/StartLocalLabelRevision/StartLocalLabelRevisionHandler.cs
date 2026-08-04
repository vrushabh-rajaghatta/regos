using RegOS.Labeling.Domain.Aggregates.LocalLabels;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.StartLocalLabelRevision;

public sealed class StartLocalLabelRevisionHandler
{
    private readonly ILocalLabelRepository _labels;

    public StartLocalLabelRevisionHandler(ILocalLabelRepository labels)
    {
        _labels = labels;
    }

    public async Task<StartLocalLabelRevisionResult> HandleAsync(
        StartLocalLabelRevisionCommand command,
        CancellationToken cancellationToken)
    {
        var label = await _labels.GetByIdAsync(
                command.LocalLabelId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.LocalLabelDoesNotExist);

        var revision = label.StartRevision();

        await _labels.UpdateAsync(label, cancellationToken);

        return new StartLocalLabelRevisionResult(
            revision.Id, revision.RevisionNumber);
    }
}
