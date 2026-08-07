using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Process.Domain.Aggregates.ProcessObjectives;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Process.Application.Commands.CreateProcessObjective;

/// <summary>
/// States what a tenant is trying to achieve in one market.
/// </summary>
/// <remarks>
/// <b>The product and country are checked; no market-local record is.</b> That is
/// the whole of [ADR-065 D8](../../../../../docs/adr/ADR-065-regulatory-process-is-an-optional-bounded-context.md):
/// an objective exists <em>before</em> the regulatory machinery for its market
/// does, and requiring a <c>MedicinalProduct</c> here would force an organisation
/// to create a regulatory artefact purely to satisfy a foreign key.
/// </remarks>
public sealed class CreateProcessObjectiveHandler
{
    private readonly IProcessObjectiveRepository _objectives;
    private readonly RegOSDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public CreateProcessObjectiveHandler(
        IProcessObjectiveRepository objectives,
        RegOSDbContext dbContext,
        ITenantContext tenantContext)
    {
        _objectives = objectives;
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<CreateProcessObjectiveResult> HandleAsync(
        CreateProcessObjectiveCommand command,
        CancellationToken cancellationToken)
    {
        // Reads, not repository calls: both belong to other contexts and this
        // handler needs only to know they exist. The query filter makes the
        // product check fail-closed (ADR-031); a country is a world fact and
        // carries no filter at all.
        var productExists = await _dbContext.Products
            .AsNoTracking()
            .AnyAsync(x => x.Id == command.GlobalProductId, cancellationToken);

        if (!productExists)
            throw new NotFoundException(ProcessObjectiveErrors.ProductRequired);

        var countryExists = await _dbContext.Countries
            .AsNoTracking()
            .AnyAsync(x => x.Id == command.CountryId, cancellationToken);

        if (!countryExists)
            throw new NotFoundException(ProcessObjectiveErrors.CountryRequired);

        var objective = ProcessObjective.Create(
            _tenantContext.TenantId,
            command.GlobalProductId,
            command.CountryId,
            command.Name,
            command.StatedOn,
            command.Rationale,
            command.OwnerUserId,
            command.TargetCompletionOn);

        await _objectives.AddAsync(objective, cancellationToken);

        return new CreateProcessObjectiveResult(objective.Id.Value);
    }
}
