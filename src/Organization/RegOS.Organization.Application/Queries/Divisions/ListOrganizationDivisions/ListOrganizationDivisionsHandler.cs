using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.Organization.Application.Queries.Divisions.ListOrganizationDivisions;

/// <summary>
/// Scoped to one organization, deliberately: unlike sites and contacts, nobody
/// asks for a directory of every division across the registry. This root is
/// justified by future by-id references, not by a cross-registry query.
/// </summary>
public sealed class ListOrganizationDivisionsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListOrganizationDivisionsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Null when the organization does not exist, so the endpoint can 404.
    /// </summary>
    public async Task<IReadOnlyList<OrganizationDivisionRow>?> HandleAsync(
        ListOrganizationDivisionsQuery query,
        CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Organizations
            .AsNoTracking()
            .AnyAsync(x => x.Id == query.OrganizationId, cancellationToken);

        if (!exists)
            return null;

        var rows = await _dbContext.OrganizationDivisions
            .AsNoTracking()
            .Where(x => x.OrganizationId == query.OrganizationId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new OrganizationDivisionRow(
                x.Id.Value, x.Name, x.Acronym, x.Status.ToString(), x.StatusDate))
            .ToList();
    }
}
