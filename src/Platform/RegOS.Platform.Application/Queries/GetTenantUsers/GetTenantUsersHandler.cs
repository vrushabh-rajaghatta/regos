using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Platform.Application.Queries.GetTenantUsers;

/// <summary>
/// A named tenant's users, read across the tenant boundary. This is the
/// platform-administrator grant (ADR-033 rule 6): the endpoint requires the
/// PlatformAdministrator policy, and this handler pairs that with
/// <c>IgnoreQueryFilters</c> — the pairing is the rule, and this handler is
/// on ADR-031's named bypass list. An explicit tenant predicate replaces the
/// filter, so "all tenants" is still not expressible here: the caller names
/// one tenant per request and gets exactly that tenant's users.
/// </summary>
public sealed class GetTenantUsersHandler
{
    private readonly RegOSDbContext _dbContext;

    public GetTenantUsersHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TenantUserListItem>> HandleAsync(
        TenantId tenantId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.UserDirectory
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId.Value)
            .OrderByDescending(x => x.CreatedOn)
            .Select(x => new TenantUserListItem(
                x.Id,
                x.FirstName,
                x.LastName,
                x.Email,
                x.Status,
                x.Role))
            .ToListAsync(cancellationToken);
    }
}
