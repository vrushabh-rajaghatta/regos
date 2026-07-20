using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
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

    public GetUserByIdHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserDetails> HandleAsync(
        GetUserByIdQuery query,
        CancellationToken cancellationToken)
    {
        var userId = query.UserId.Value;

        var users = _dbContext.UserDirectory
            .AsNoTracking()
            .Where(x => x.Id == userId);

        // Tenant isolation: a user in another organization is indistinguishable
        // from one that does not exist.
        if (query.OrganizationId is not null)
        {
            var organizationId = query.OrganizationId.Value;
            users = users.Where(x => x.OrganizationId == organizationId);
        }

        var user = await users
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
