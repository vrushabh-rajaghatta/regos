using RegOS.Labeling.Domain.Aggregates.GlobalLabels;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.PublishGlobalLabelVersion;

public sealed class PublishGlobalLabelVersionHandler
{
    private readonly IGlobalLabelRepository _labels;

    public PublishGlobalLabelVersionHandler(IGlobalLabelRepository labels)
    {
        _labels = labels;
    }

    public async Task HandleAsync(
        PublishGlobalLabelVersionCommand command,
        CancellationToken cancellationToken)
    {
        var label = await _labels.GetByIdAsync(
                command.GlobalLabelId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.GlobalLabelDoesNotExist);

        // Both calls, one transaction. The summary is written while the version
        // is still a draft — a moment later the aggregate freezes it — so the
        // order here is not stylistic.
        label.SummariseChanges(command.VersionId, command.ChangeSummary);

        label.PublishVersion(
            command.VersionId,
            command.EffectiveFrom,
            DateTime.UtcNow);

        await _labels.UpdateAsync(label, cancellationToken);
    }
}
