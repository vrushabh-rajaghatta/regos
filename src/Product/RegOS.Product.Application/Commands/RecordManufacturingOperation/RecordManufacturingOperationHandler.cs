using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Application.Services;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.RecordManufacturingOperation;

public sealed class RecordManufacturingOperationHandler
{
    private readonly IManufacturingOperationRepository _operations;
    private readonly RegOSDbContext _dbContext;
    private readonly ITenantContext _tenant;

    public RecordManufacturingOperationHandler(
        IManufacturingOperationRepository operations,
        RegOSDbContext dbContext,
        ITenantContext tenant)
    {
        _operations = operations;
        _dbContext = dbContext;
        _tenant = tenant;
    }

    /// <remarks>
    /// <b>Both ends are checked before either is used</b>, so a wrong id is a
    /// 404 naming which one rather than a foreign-key violation naming neither.
    /// Both reads go through the fail-closed filters, so another tenant's market
    /// or site is <em>not found</em> rather than refused (ADR-031) — the same
    /// call <c>AuthorisePackHandler</c> makes, and the reason a site is read
    /// here through the <c>DbContext</c> rather than through Organization's
    /// repository: this is a read, and reads compose across contexts (ADR-016).
    /// </remarks>
    public async Task<ManufacturingOperationId> HandleAsync(
        RecordManufacturingOperationCommand command,
        CancellationToken cancellationToken)
    {
        var marketExists = await _dbContext.MedicinalProducts
            .AsNoTracking()
            .AnyAsync(x => x.Id == command.MedicinalProductId, cancellationToken);

        if (!marketExists)
            throw new NotFoundException(ManufacturingOperationErrors.MarketNotFound);

        var siteExists = await _dbContext.OrganizationSites
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == command.OrganizationSiteId, cancellationToken);

        if (!siteExists)
            throw new NotFoundException(
                ManufacturingOperationErrors.SiteBelongsToAnotherTenant);

        var operation = ProductVocabulary.ManufacturingOperation(
            command.OperationCode);

        // The check the filtered unique index also makes. Both are wanted: this
        // one names the act a person just attempted, the index closes the race
        // between two requests arriving together.
        var current = await _operations.GetCurrentAsync(
            command.MedicinalProductId,
            command.OrganizationSiteId,
            operation.Code,
            cancellationToken);

        if (current is not null)
            throw new BusinessRuleViolationException(
                ManufacturingOperationErrors.AlreadyPerformedHere);

        var recorded = ManufacturingOperation.Record(
            _tenant.TenantId,
            command.MedicinalProductId,
            command.OrganizationSiteId,
            operation,
            command.EffectiveFrom);

        await _operations.AddAsync(recorded, cancellationToken);

        return recorded.Id;
    }
}
