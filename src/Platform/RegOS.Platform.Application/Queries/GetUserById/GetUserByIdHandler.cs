using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Platform.Application.Queries.GetUserById;

/// <summary>
/// Reads a single user straight from the database: no repository, no aggregate,
/// no tracking. Projects from the flat directory read model so the query stays
/// fully translatable, exactly like the user list.
/// </summary>
public sealed class GetUserByIdHandler
{
    private readonly RegOSDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public GetUserByIdHandler(
        RegOSDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<UserDetails> HandleAsync(
        GetUserByIdQuery query,
        CancellationToken cancellationToken)
    {
        var userId = query.UserId.Value;
        var tenantId = _tenantContext.TenantId;

        // Tenant isolation: a user in another organization is indistinguishable
        // from one that does not exist. Applied unconditionally - there is no
        // longer a code path that reads across tenants.
        var user = await _dbContext.UserDirectory
            .AsNoTracking()
            .Where(x => x.Id == userId && x.OrganizationId == tenantId)
            .Select(x => new UserDetails(
                x.Id,
                x.FirstName,
                x.LastName,
                x.Email,
                x.Status,
                x.CreatedOn))
            .SingleOrDefaultAsync(cancellationToken);

        return user
            ?? throw new NotFoundException(PlatformErrors.UserNotFound);
    }
}
