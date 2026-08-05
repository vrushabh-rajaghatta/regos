using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.Persistence;
using RegOS.Product.Domain.Product;

namespace RegOS.Product.Infrastructure.Persistence;

public sealed class ManufacturingOperationRepository
    : IManufacturingOperationRepository
{
    private readonly RegOSDbContext _dbContext;

    public ManufacturingOperationRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        ManufacturingOperation operation,
        CancellationToken cancellationToken)
    {
        _dbContext.ManufacturingOperations.Add(operation);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <remarks>
    /// No <c>Include</c>: the aggregate holds two ids, a coded value and two
    /// dates, and reasons across nothing.
    /// </remarks>
    public async Task<ManufacturingOperation?> GetByIdAsync(
        ManufacturingOperationId id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.ManufacturingOperations
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <remarks>
    /// <b>The check the database also makes</b>, and both are wanted: this one
    /// produces a sentence a user can act on, the filtered unique index behind
    /// it closes the race between two requests arriving together.
    /// </remarks>
    public async Task<ManufacturingOperation?> GetCurrentAsync(
        MedicinalProductId medicinalProductId,
        OrganizationSiteId organizationSiteId,
        string operationCode,
        CancellationToken cancellationToken)
    {
        return await _dbContext.ManufacturingOperations
            .FirstOrDefaultAsync(
                x => x.MedicinalProductId == medicinalProductId
                    && x.OrganizationSiteId == organizationSiteId
                    && x.Operation.Code == operationCode
                    && x.CeasedOn == null,
                cancellationToken);
    }

    public async Task UpdateAsync(
        ManufacturingOperation operation,
        CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
