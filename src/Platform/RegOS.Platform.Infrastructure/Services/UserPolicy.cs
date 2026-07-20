using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Persistence;
using RegOS.Platform.Application;
using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;
using RegOS.SharedKernel.Exceptions;

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

        // The organization the caller named does not exist, so the *request* is
        // wrong (400) rather than the system state being in conflict (409).
        // Matches how RegulatoryApplication classifies the same condition.
        if (organization is null)
            throw new DomainException(
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

    public async Task EnsureEmailIsUniqueForUpdateAsync(
        OrganizationId organizationId,
        UserId userId,
        Email email,
        CancellationToken cancellationToken)
    {
        // Identical to the invite rule, except the user being updated is not
        // allowed to collide with itself.
        var alreadyInUse = await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(
                x => x.OrganizationId == organizationId
                    && x.Email == email
                    && x.Id != userId,
                cancellationToken);

        if (alreadyInUse)
            throw new BusinessRuleViolationException(
                PlatformErrors.EmailAlreadyInUse);
    }
}
