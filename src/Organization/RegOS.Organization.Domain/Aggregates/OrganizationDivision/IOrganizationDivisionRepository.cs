namespace RegOS.Organization.Domain.Aggregates.OrganizationDivision;

/// <summary>
/// Aggregates only. Reads for screens project from <c>RegOSDbContext</c>
/// directly with <c>AsNoTracking()</c> (ADR-016).
/// </summary>
public interface IOrganizationDivisionRepository
{
    Task AddAsync(
        OrganizationDivision division,
        CancellationToken cancellationToken);

    Task<OrganizationDivision?> GetByIdAsync(
        OrganizationDivisionId id,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        OrganizationDivision division,
        CancellationToken cancellationToken);
}
