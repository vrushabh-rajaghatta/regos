using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.SharedKernel.Exceptions;

using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;

namespace RegOS.Platform.Application.Common;

internal static class UserRepositoryExtensions
{
    /// <summary>
    /// Loads a user that must exist and must belong to the caller's tenant.
    /// Extracted once three commands needed the identical lookup (update
    /// profile, activate, deactivate).
    /// </summary>
    /// <remarks>
    /// The organization is non-nullable by design. It was optional while the
    /// tenant travelled as a query-string parameter, which meant omitting it
    /// silently disabled isolation; now the tenant is always known, so there is
    /// no "unscoped" call to express. A user belonging to another tenant is
    /// reported as not found rather than forbidden, so the API never reveals
    /// that the record exists.
    /// </remarks>
    public static async Task<UserAggregate> GetRequiredAsync(
        this IUserRepository repository,
        UserId userId,
        OrganizationId organizationId,
        CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(userId, cancellationToken);

        if (user is null || user.OrganizationId != organizationId)
            throw new NotFoundException(PlatformErrors.UserNotFound);

        return user;
    }
}
