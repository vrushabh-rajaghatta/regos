using Microsoft.EntityFrameworkCore;

using RegOS.Labeling.Domain.Aggregates.Indications;
using RegOS.Persistence;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.RecordIndication;

public sealed class RecordIndicationHandler
{
    private readonly IIndicationRepository _indications;
    private readonly RegOSDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public RecordIndicationHandler(
        IIndicationRepository indications,
        RegOSDbContext dbContext,
        ITenantContext tenantContext)
    {
        _indications = indications;
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<RecordIndicationResult> HandleAsync(
        RecordIndicationCommand command,
        CancellationToken cancellationToken)
    {
        // A read, not a repository call — the market is another context's
        // aggregate, and the query filter makes the check fail-closed.
        var marketExists = await _dbContext.MedicinalProducts
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == command.MedicinalProductId, cancellationToken);

        if (!marketExists)
            throw new NotFoundException(
                LabelingRuleErrors.MedicinalProductDoesNotExist);

        var indication = Indication.Record(
            _tenantContext.TenantId,
            command.MedicinalProductId,
            command.ConditionCode,
            command.LabelText,
            command.ApprovedOn,
            DateTime.UtcNow);

        await _indications.AddAsync(indication, cancellationToken);

        return new RecordIndicationResult(indication.Id);
    }
}
