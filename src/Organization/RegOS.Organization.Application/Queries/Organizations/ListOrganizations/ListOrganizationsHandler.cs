using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.Organization.Application.Queries.Organizations.ListOrganizations;

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
            .Select(x => new OrganizationDto(
                x.Id,
                x.LegalName,
                x.Type,
                x.Status))
            .ToListAsync(cancellationToken);
    }
}
