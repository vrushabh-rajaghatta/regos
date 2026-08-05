using RegOS.Organization.Domain.Aggregates.OrganizationSite;

namespace RegOS.Product.Domain.Product;

public interface IManufacturingOperationRepository
{
    Task AddAsync(
        ManufacturingOperation operation,
        CancellationToken cancellationToken);

    /// <summary>Tracked — for mutation.</summary>
    Task<ManufacturingOperation?> GetByIdAsync(
        ManufacturingOperationId id,
        CancellationToken cancellationToken);

    /// <summary>
    /// The open period for this (market, site, operation), if one exists.
    /// </summary>
    /// <remarks>
    /// <b>Open, not any.</b> The same site may perform the same operation over
    /// two separate periods — transferred away and brought back is ordinary —
    /// so the invariant is one <em>current</em> row, not one row ever.
    /// </remarks>
    Task<ManufacturingOperation?> GetCurrentAsync(
        MedicinalProductId medicinalProductId,
        OrganizationSiteId organizationSiteId,
        string operationCode,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        ManufacturingOperation operation,
        CancellationToken cancellationToken);
}
