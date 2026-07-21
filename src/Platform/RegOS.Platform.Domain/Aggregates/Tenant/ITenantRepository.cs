namespace RegOS.Platform.Domain.Aggregates.Tenant;

using RegOS.SharedKernel.Primitives;

/// <summary>
/// Aggregates only (ADR-016). Reads for screens project from the database
/// directly rather than loading aggregates through here.
/// </summary>
public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(
        TenantId id,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        Tenant tenant,
        CancellationToken cancellationToken);
}
