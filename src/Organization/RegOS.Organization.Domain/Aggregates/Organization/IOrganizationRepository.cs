using OrganizationAggregate =
    RegOS.Organization.Domain.Aggregates.Organization.Organization;

namespace RegOS.Organization.Domain.Aggregates.Organization;

/// <summary>
/// Aggregates only. Reads for screens project from the database directly rather
/// than loading aggregates through here (ADR-006, ADR-016).
/// </summary>
public interface IOrganizationRepository
{
    Task AddAsync(
        OrganizationAggregate organization,
        CancellationToken cancellationToken);

    /// <summary>
    /// Loads the organization with its identifiers — the whole aggregate.
    /// <c>AddIdentifier</c> refuses a scheme the company already holds, and it
    /// can only see the ones that were loaded; a partial load would let the
    /// duplicate through to the database, where the unique index turns a clear
    /// business rule into a raw persistence failure.
    /// </summary>
    Task<OrganizationAggregate?> GetByIdAsync(
        OrganizationId id,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        OrganizationAggregate organization,
        CancellationToken cancellationToken);
}
