using Microsoft.EntityFrameworkCore;

using RegOS.Labeling.Domain.Aggregates.LocalLabels;
using RegOS.Persistence;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.CreateLocalLabel;

public sealed class CreateLocalLabelHandler
{
    private readonly ILocalLabelRepository _labels;
    private readonly RegOSDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public CreateLocalLabelHandler(
        ILocalLabelRepository labels,
        RegOSDbContext dbContext,
        ITenantContext tenantContext)
    {
        _labels = labels;
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<CreateLocalLabelResult> HandleAsync(
        CreateLocalLabelCommand command,
        CancellationToken cancellationToken)
    {
        // A read, not a repository call: the market belongs to another context
        // and this handler needs only to know one exists and is ours. The query
        // filter makes the check fail-closed (ADR-031).
        var marketExists = await _dbContext.MedicinalProducts
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == command.MedicinalProductId, cancellationToken);

        if (!marketExists)
            throw new NotFoundException(
                LabelingRuleErrors.MedicinalProductDoesNotExist);

        var label = LocalLabel.Create(
            _tenantContext.TenantId,
            command.MedicinalProductId,
            command.LabelTypeCode,
            command.Language,
            DateTime.UtcNow);

        await _labels.AddAsync(label, cancellationToken);

        return new CreateLocalLabelResult(label.Id, label.Draft!.Id);
    }
}
