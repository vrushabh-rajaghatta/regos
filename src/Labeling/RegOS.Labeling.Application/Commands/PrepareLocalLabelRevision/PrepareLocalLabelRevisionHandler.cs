using Microsoft.EntityFrameworkCore;

using RegOS.Labeling.Domain.Aggregates.LocalLabels;
using RegOS.Persistence;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.PrepareLocalLabelRevision;

public sealed class PrepareLocalLabelRevisionHandler
{
    private readonly ILocalLabelRepository _labels;
    private readonly RegOSDbContext _dbContext;

    public PrepareLocalLabelRevisionHandler(
        ILocalLabelRepository labels,
        RegOSDbContext dbContext)
    {
        _labels = labels;
        _dbContext = dbContext;
    }

    public async Task HandleAsync(
        PrepareLocalLabelRevisionCommand command,
        CancellationToken cancellationToken)
    {
        var label = await _labels.GetByIdAsync(
                command.LocalLabelId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.LocalLabelDoesNotExist);

        if (command.ContentId is { } contentId)
        {
            // ADR-059 §6 again, one tier down: Labeling asks ProductDocument a
            // question and writes nothing to it. The document must be ours and
            // held for the global product this market localises — which this
            // handler establishes by joining through the market rather than by
            // trusting the caller.
            var product = await _dbContext.MedicinalProducts
                .AsNoTracking()
                .Where(x => x.Id == label.MedicinalProductId)
                .Select(x => x.GlobalProductId)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException(
                    LabelingRuleErrors.MedicinalProductDoesNotExist);

            var document = await _dbContext.ProductDocuments
                .AsNoTracking()
                .Where(x => x.Id == contentId)
                .Select(x => new { x.GlobalProductId })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException(
                    LabelingRuleErrors.ContentDoesNotExist);

            if (document.GlobalProductId != product)
                throw new BusinessRuleViolationException(
                    LabelingRuleErrors.ContentBelongsToAnotherProduct);
        }

        label.PrepareRevision(
            command.RevisionId,
            command.ContentId,
            command.DerivedFromGlobalLabelVersionId,
            command.DataCarrierCode,
            command.ChangeSummary);

        await _labels.UpdateAsync(label, cancellationToken);
    }
}
