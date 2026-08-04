using Microsoft.EntityFrameworkCore;

using RegOS.Labeling.Domain.Aggregates.DrugInteractions;
using RegOS.Persistence;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Labeling.Application.Commands.RecordDrugInteraction;

public sealed class RecordDrugInteractionHandler
{
    private readonly IDrugInteractionRepository _interactions;
    private readonly RegOSDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public RecordDrugInteractionHandler(
        IDrugInteractionRepository interactions,
        RegOSDbContext dbContext,
        ITenantContext tenantContext)
    {
        _interactions = interactions;
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<RecordDrugInteractionResult> HandleAsync(
        RecordDrugInteractionCommand command,
        CancellationToken cancellationToken)
    {
        var marketExists = await _dbContext.MedicinalProducts
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == command.MedicinalProductId, cancellationToken);

        if (!marketExists)
            throw new NotFoundException(
                LabelingRuleErrors.MedicinalProductDoesNotExist);

        // The seam is a real foreign key, so a bad id would fail at save time as
        // a 500. Checked here instead, and against the shared-plus-extensible
        // filter, so another tenant's proprietary compound reads as absent.
        if (command.InteractantSubstanceId is { } substanceId)
        {
            var known = await _dbContext.Substances
                .AsNoTracking()
                .AnyAsync(x => x.Id == substanceId, cancellationToken);

            if (!known)
                throw new NotFoundException(
                    LabelingRuleErrors.SubstanceDoesNotExist);
        }

        var interaction = DrugInteraction.Record(
            _tenantContext.TenantId,
            command.MedicinalProductId,
            command.InteractionTypeCode,
            command.LabelText,
            command.Interactant,
            command.InteractantSubstanceId,
            command.Management,
            command.SeverityCode,
            DateTime.UtcNow);

        await _interactions.AddAsync(interaction, cancellationToken);

        return new RecordDrugInteractionResult(interaction.Id);
    }
}
