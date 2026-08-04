using Microsoft.EntityFrameworkCore;

using RegOS.Labeling.Domain.Aggregates.Contraindications;
using RegOS.Persistence;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.RecordContraindication;

public sealed class RecordContraindicationHandler
{
    private readonly IContraindicationRepository _statements;
    private readonly RegOSDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public RecordContraindicationHandler(
        IContraindicationRepository statements,
        RegOSDbContext dbContext,
        ITenantContext tenantContext)
    {
        _statements = statements;
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<RecordContraindicationResult> HandleAsync(
        RecordContraindicationCommand command,
        CancellationToken cancellationToken)
    {
        // A read, not a repository call: the market is another context's
        // aggregate, and the query filter makes the check fail-closed.
        var marketExists = await _dbContext.MedicinalProducts
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == command.MedicinalProductId, cancellationToken);

        if (!marketExists)
            throw new NotFoundException(
                LabelingRuleErrors.MedicinalProductDoesNotExist);

        var statement = Contraindication.Record(
            _tenantContext.TenantId,
            command.MedicinalProductId,
            command.ConditionCode,
            command.LabelText,
            DateTime.UtcNow);

        await _statements.AddAsync(statement, cancellationToken);

        return new RecordContraindicationResult(statement.Id);
    }
}
