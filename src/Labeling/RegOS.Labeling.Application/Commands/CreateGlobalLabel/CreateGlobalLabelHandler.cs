using Microsoft.EntityFrameworkCore;

using RegOS.Labeling.Domain.Aggregates.GlobalLabels;
using RegOS.Persistence;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.CreateGlobalLabel;

public sealed class CreateGlobalLabelHandler
{
    private readonly IGlobalLabelRepository _labels;
    private readonly RegOSDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public CreateGlobalLabelHandler(
        IGlobalLabelRepository labels,
        RegOSDbContext dbContext,
        ITenantContext tenantContext)
    {
        _labels = labels;
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<CreateGlobalLabelResult> HandleAsync(
        CreateGlobalLabelCommand command,
        CancellationToken cancellationToken)
    {
        // Read, not a repository call. The product belongs to another context
        // and this handler has no business loading its aggregate — it only
        // needs to know one exists and is ours. The query filter makes that
        // check fail-closed, so a valid id from another tenant reads as absent
        // rather than as forbidden (ADR-031).
        var productExists = await _dbContext.Products
            .AsNoTracking()
            .AnyAsync(x => x.Id == command.GlobalProductId, cancellationToken);

        if (!productExists)
            throw new NotFoundException(
                LabelingRuleErrors.GlobalProductDoesNotExist);

        var label = GlobalLabel.Create(
            _tenantContext.TenantId,
            command.GlobalProductId,
            command.Name,
            command.LabelTypeCode,
            DateTime.UtcNow);

        await _labels.AddAsync(label, cancellationToken);

        return new CreateGlobalLabelResult(label.Id, label.Draft!.Id);
    }
}
