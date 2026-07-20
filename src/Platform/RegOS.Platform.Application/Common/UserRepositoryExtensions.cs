using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Platform.Application.Exceptions;
using RegOS.Platform.Domain.Aggregates.User;

using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;

namespace RegOS.Platform.Application.Common;

internal static class UserRepositoryExtensions
{
    /// <summary>
    /// Loads a user that must exist and must be visible to the caller's
    /// organization. Extracted once three commands needed the identical lookup
    /// (update profile, activate, deactivate).
    /// </summary>
    /// <remarks>
    /// A user belonging to another organization is reported as not found rather
    /// than forbidden, so the API never reveals that the record exists.
    /// </remarks>
    public static async Task<UserAggregate> GetRequiredAsync(
        this IUserRepository repository,
        UserId userId,
        OrganizationId? organizationId,
        CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(userId, cancellationToken);

        if (user is null
            || (organizationId is not null
                && user.OrganizationId != organizationId))
            throw new NotFoundException(PlatformErrors.UserNotFound);

        return user;
    }
}
