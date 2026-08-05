using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Platform.Application.Common;
using RegOS.SharedKernel.Abstractions;

namespace RegOS.Platform.Application.Queries.GetUsers;

/// <summary>
/// Reads the user directory straight from the database. This is reporting, not
/// domain modelling: no repository, no aggregate loading, no tracking, no
/// Include — only the columns the directory screen needs, projected from a flat
/// read model rather than through the User aggregate's value converters.
/// </summary>
public sealed class GetUsersHandler
{
    private readonly RegOSDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public GetUsersHandler(
        RegOSDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<PagedResult<UserListItem>> HandleAsync(
        GetUsersQuery query,
        CancellationToken cancellationToken)
    {
        // Clamp rather than reject: a caller asking for page 0 or 5000 rows gets
        // a sensible page, never an unbounded read.
        var page = query.Page < 1 ? GetUsersQuery.DefaultPage : query.Page;
        var pageSize = Math.Clamp(
            query.PageSize, 1, GetUsersQuery.MaxPageSize);

        // Tenant filter first, and unconditionally. There is no branch that can
        // skip it, which is the entire point: a directory read cannot be
        // widened past the caller's own tenant.
        var tenantId = _tenantContext.TenantId.Value;

        var users = _dbContext.UserDirectory
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (query.Status is not null)
        {
            var status = query.Status.Value;
            users = users.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            // One search box across first name, last name and email.
            var pattern = $"%{query.Search.Trim()}%";

            users = users.Where(x =>
                EF.Functions.ILike(x.FirstName, pattern)
                || EF.Functions.ILike(x.LastName, pattern)
                || EF.Functions.ILike(x.Email, pattern));
        }

        var totalCount = await users.CountAsync(cancellationToken);

        var items = await users
            // Paged: a tie here would move a user between pages, so the id
            // is not decoration.
            .OrderByDescending(x => x.CreatedOn) // newest invitations first
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserListItem(
                x.Id,
                x.FirstName,
                x.LastName,
                x.Email,
                x.Status,
                x.CreatedOn))
            .ToListAsync(cancellationToken);

        return new PagedResult<UserListItem>(
            items, totalCount, page, pageSize);
    }
}
