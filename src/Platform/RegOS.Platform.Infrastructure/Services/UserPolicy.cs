using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Persistence;
using RegOS.Platform.Application;
using RegOS.Platform.Application.Exceptions;
using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.ValueObjects;

namespace RegOS.Platform.Infrastructure.Services;

public sealed class UserPolicy : IUserPolicy
{
    private readonly RegOSDbContext _dbContext;

    public UserPolicy(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureOrganizationCanAcceptUsersAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken)
    {
        var organization = await _dbContext.Organizations
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == organizationId, cancellationToken);

        if (organization is null)
            throw new BusinessRuleViolationException(
                PlatformErrors.OrganizationDoesNotExist);

        if (organization.Status != OrganizationStatus.Active)
            throw new BusinessRuleViolationException(
                PlatformErrors.OrganizationInactive);
    }

    public async Task EnsureEmailIsUniqueAsync(
        OrganizationId organizationId,
        Email email,
        CancellationToken cancellationToken)
    {
        var alreadyInUse = await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(
                x => x.OrganizationId == organizationId && x.Email == email,
                cancellationToken);

        if (alreadyInUse)
            throw new BusinessRuleViolationException(
                PlatformErrors.EmailAlreadyInUse);
    }
}
