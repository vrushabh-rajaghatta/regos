using Microsoft.EntityFrameworkCore;

using RegOS.Labeling.Domain.Aggregates.GlobalLabels;
using RegOS.Persistence;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.AttachGlobalLabelContent;

public sealed class AttachGlobalLabelContentHandler
{
    private readonly IGlobalLabelRepository _labels;
    private readonly RegOSDbContext _dbContext;

    public AttachGlobalLabelContentHandler(
        IGlobalLabelRepository labels,
        RegOSDbContext dbContext)
    {
        _labels = labels;
        _dbContext = dbContext;
    }

    public async Task HandleAsync(
        AttachGlobalLabelContentCommand command,
        CancellationToken cancellationToken)
    {
        var label = await _labels.GetByIdAsync(
                command.GlobalLabelId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.GlobalLabelDoesNotExist);

        // The whole of ADR-059 §6 in one read: Labeling asks ProductDocument a
        // question and writes nothing to it. The document keeps its own
        // lifecycle, its own versions and its own context — this handler only
        // establishes that the file being named is a real one, ours, and held
        // for the same product the label is.
        var document = await _dbContext.ProductDocuments
            .AsNoTracking()
            .Where(x => x.Id == command.ContentId)
            .Select(x => new { x.GlobalProductId })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.ContentDoesNotExist);

        if (document.GlobalProductId != label.GlobalProductId)
            throw new BusinessRuleViolationException(
                LabelingRuleErrors.ContentBelongsToAnotherProduct);

        label.AttachContent(command.VersionId, command.ContentId);

        await _labels.UpdateAsync(label, cancellationToken);
    }
}
