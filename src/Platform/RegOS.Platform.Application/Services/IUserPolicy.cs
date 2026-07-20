using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;

namespace RegOS.Platform.Application.Services;

/// <summary>
/// Business rules for users that require asking the outside world — distinct from
/// the aggregate's own invariants, which live in the domain. Worded as
/// <c>Ensure…</c> so the handler reads as intent: the method either lets the flow
/// continue or throws a <see cref="Exceptions.BusinessRuleViolationException"/>.
/// </summary>
public interface IUserPolicy
{
    Task EnsureOrganizationCanAcceptUsersAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken);

    Task EnsureEmailIsUniqueAsync(
        OrganizationId organizationId,
        Email email,
        CancellationToken cancellationToken);

    /// <summary>
    /// Same rule as <see cref="EnsureEmailIsUniqueAsync"/>, but ignores the user
    /// being updated — otherwise saving a profile without changing the email
    /// would collide with itself.
    /// </summary>
    Task EnsureEmailIsUniqueForUpdateAsync(
        OrganizationId organizationId,
        UserId userId,
        Email email,
        CancellationToken cancellationToken);
}
