using Microsoft.EntityFrameworkCore;

using RegOS.Labeling.Domain.Aggregates.LocalLabels;
using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.PrintLocalLabelForPack;

public sealed class PrintLocalLabelForPackHandler
{
    private readonly ILocalLabelRepository _labels;
    private readonly RegOSDbContext _dbContext;

    public PrintLocalLabelForPackHandler(
        ILocalLabelRepository labels,
        RegOSDbContext dbContext)
    {
        _labels = labels;
        _dbContext = dbContext;
    }

    public async Task HandleAsync(
        PrintLocalLabelForPackCommand command,
        CancellationToken cancellationToken)
    {
        var label = await _labels.GetByIdAsync(
                command.LocalLabelId, cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.LocalLabelDoesNotExist);

        if (command.PackagedProductId is null)
        {
            label.PrintedFor(null);

            await _labels.UpdateAsync(label, cancellationToken);

            return;
        }

        var packId = PackagedProductId.From(command.PackagedProductId.Value);

        // Labeling asks Product a question and writes nothing to it — the same
        // read AttachGlobalLabelContent makes of ProductDocument (ADR-059 §6).
        // It establishes only that the pack is real, ours, and sold in the same
        // market this label belongs to.
        var pack = await _dbContext.PackagedProducts
            .AsNoTracking()
            .Where(x => x.Id == packId)
            .Select(x => new { x.MedicinalProductId })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(
                LabelingRuleErrors.PackDoesNotExist);

        // A French carton naming a UK pack is the mistake worth catching: both
        // rows exist, both belong to the tenant, and nothing else would notice.
        if (pack.MedicinalProductId != label.MedicinalProductId)
            throw new BusinessRuleViolationException(
                LabelingRuleErrors.PackBelongsToAnotherMarket);

        label.PrintedFor(packId);

        await _labels.UpdateAsync(label, cancellationToken);
    }
}
