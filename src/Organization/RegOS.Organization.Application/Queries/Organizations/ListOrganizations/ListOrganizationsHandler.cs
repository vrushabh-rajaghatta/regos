using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.Organization.Application.Queries.Organizations.ListOrganizations;

/// <summary>
/// Lists the caller's own registry. No manual tenant clause — the global
/// query filter does the scoping (ADR-032), so this deliberately bare query
/// returns each tenant its own organizations and nothing else.
/// </summary>
public sealed class ListOrganizationsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListOrganizationsHandler(
        RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<OrganizationDto>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Organizations
            .AsNoTracking()
            .OrderBy(x => x.LegalName)
            .ThenBy(x => x.Id)
            .Select(x => new OrganizationDto(
                x.Id,
                x.LegalName,
                x.Type,
                x.Status))
            .ToListAsync(cancellationToken);
    }
}
