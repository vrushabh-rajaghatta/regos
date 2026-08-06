using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Process.Domain.Aggregates.ProcessObjectives;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Process.Application.Commands.ConfirmObjectiveMarketRecord;

/// <summary>
/// <b>Where ADR-065 D8's invariant is enforced.</b>
/// </summary>
/// <remarks>
/// The rule: <em>once <c>MedicinalProductId</c> is populated it must reference a
/// record whose global product and country are the ones this objective already
/// holds.</em> The link <b>confirms identity; it never redefines it</b> — and it
/// is what stops the duplicated <c>(product, country)</c> pair drifting.
/// <para>
/// <b>The rule is the domain's and this is where it runs</b>, because checking it
/// means loading a <c>MedicinalProduct</c> — the cross-aggregate read
/// [ADR-016](../../../../../docs/adr/ADR-016-persistence-access-model.md) keeps
/// out of an aggregate. <c>ProcessObjective.ConfirmMarketRecord</c> documents the
/// precondition it is handed; <c>LocalLabel.PrintedFor</c> carries the identical
/// note for packs, and EPIC-010b resolved it the same way.
/// </para>
/// <para>
/// <b>Clearing the link is unconditional.</b> Passing null says <em>"this is no
/// longer the record that fulfils the objective"</em>, which needs no market to
/// verify against and must stay possible when the record it pointed at was
/// retired.
/// </para>
/// </remarks>
public sealed class ConfirmObjectiveMarketRecordHandler
{
    private readonly IProcessObjectiveRepository _objectives;
    private readonly RegOSDbContext _dbContext;

    public ConfirmObjectiveMarketRecordHandler(
        IProcessObjectiveRepository objectives,
        RegOSDbContext dbContext)
    {
        _objectives = objectives;
        _dbContext = dbContext;
    }

    public async Task HandleAsync(
        ConfirmObjectiveMarketRecordCommand command,
        CancellationToken cancellationToken)
    {
        var objective =
            await _objectives.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("That objective does not exist.");

        if (command.MedicinalProductId is { } marketRecordId)
        {
            // AsNoTracking and two columns: this is a check, not a load. The
            // query filter makes it fail-closed — another tenant's market record
            // is indistinguishable from one that does not exist, which is the
            // correct answer to give (ADR-031).
            var market = await _dbContext.MedicinalProducts
                .AsNoTracking()
                .Where(x => x.Id == marketRecordId)
                .Select(x => new { x.GlobalProductId, x.CountryId })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException(
                    ProcessObjectiveErrors.MarketRecordNotFound);

            if (market.GlobalProductId != objective.GlobalProductId
                || market.CountryId != objective.CountryId)
            {
                throw new BusinessRuleViolationException(
                    ProcessObjectiveErrors.MarketRecordIsForAnotherMarket);
            }
        }

        objective.ConfirmMarketRecord(command.MedicinalProductId);

        await _objectives.UpdateAsync(objective, cancellationToken);
    }
}
