using RegOS.Organization.Domain.Aggregates.Organization;

using OrganizationAggregate =
    RegOS.Organization.Domain.Aggregates.Organization.Organization;

namespace RegOS.Organization.Application.Persistence;

/// <summary>
/// Aggregates only. Reads for screens project from the database directly rather
/// than loading aggregates through here (ADR-006, ADR-016).
/// </summary>
public interface IOrganizationRepository
{
    Task AddAsync(
        OrganizationAggregate organization,
        CancellationToken cancellationToken);

    Task<OrganizationAggregate?> GetByIdAsync(
        OrganizationId id,
        CancellationToken cancellationToken);
}
