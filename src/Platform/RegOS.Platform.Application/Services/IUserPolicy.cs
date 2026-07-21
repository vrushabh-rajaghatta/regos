using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Platform.Application.Services;

/// <summary>
/// Business rules for users that require asking the outside world — distinct from
/// the aggregate's own invariants, which live in the domain. Worded as
/// <c>Ensure…</c> so the handler reads as intent: the method either lets the flow
/// continue or throws a <see cref="Exceptions.BusinessRuleViolationException"/>.
/// </summary>
public interface IUserPolicy
{
    Task EnsureTenantCanAcceptUsersAsync(
        TenantId tenantId,
        CancellationToken cancellationToken);

    /// <summary>
    /// An email address identifies exactly one user across RegOS, so this rule
    /// takes no organization (ADR-021). The parameter was removed rather than
    /// ignored: authentication resolves a user before any tenant exists, and a
    /// per-organization check could not answer it.
    /// </summary>
    Task EnsureEmailIsUniqueAsync(
        Email email,
        CancellationToken cancellationToken);

    /// <summary>
    /// Same rule as <see cref="EnsureEmailIsUniqueAsync"/>, but ignores the user
    /// being updated — otherwise saving a profile without changing the email
    /// would collide with itself.
    /// </summary>
    Task EnsureEmailIsUniqueForUpdateAsync(
        UserId userId,
        Email email,
        CancellationToken cancellationToken);
}
