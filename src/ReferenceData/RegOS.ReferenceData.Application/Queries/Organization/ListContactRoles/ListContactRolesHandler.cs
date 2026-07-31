using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.ReferenceData.Application.Queries.Organization.ListContactRoles;

/// <summary>
/// The roles a contact can hold.
/// </summary>
/// <remarks>
/// No tenant clause here: <c>ContactRole</c> is shared-plus-extensible, and its
/// query filter already returns the platform's roles plus this tenant's own
/// (the second filter shape in <c>RegOSDbContext</c>'s remarks). Restating it
/// would be a second place to keep right.
/// </remarks>
public sealed class ListContactRolesHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListContactRolesHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ContactRoleDto>> HandleAsync(
        ListContactRolesQuery query,
        CancellationToken cancellationToken)
        => await _dbContext.ContactRoles
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ContactRoleDto(
                x.Id.Value,
                x.Code,
                x.Name,
                x.Description,
                x.TenantId != null))
            .ToListAsync(cancellationToken);
}
