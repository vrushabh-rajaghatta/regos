using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;

namespace RegOS.Platform.Application.Services;

/// <summary>
/// The authenticated caller behind the current request.
/// </summary>
/// <remarks>
/// <para>
/// Deliberately four members and no more. The pressure on a type like this is
/// always to grow — a display name here, a role there, the organization's name
/// because a page needs it — until every service depends on it and none of them
/// can say why. Everything absent from this interface can be resolved from
/// <see cref="UserId"/> by whoever actually needs it.
/// </para>
/// <para>
/// Roles and permissions are absent for a second reason: they are not decided
/// yet (Epic 4). Putting them here now would mean guessing their shape and
/// having every caller inherit the guess.
/// </para>
/// <para>
/// This is a sibling of <c>ITenantContext</c>, not a replacement. That answers
/// <em>which tenant is this request scoped to</em>, which is an infrastructure
/// question every context asks; this answers <em>which person is calling</em>,
/// which is a Platform concept and typed accordingly. They agree today because
/// a user belongs to exactly one organization (ADR-015).
/// </para>
/// </remarks>
public interface ICurrentUser
{
    /// <summary>
    /// Whether the request carried a valid token. This is the only member safe
    /// to read on an anonymous request; the others throw.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// The caller's user id. Throws when the request is not authenticated,
    /// rather than returning an empty id — the same reasoning as
    /// <c>ITenantContext.TenantId</c>: an unauthenticated default must never be
    /// mistakable for a real caller.
    /// </summary>
    UserId UserId { get; }

    /// <summary>
    /// The organization the caller belongs to. Throws when unauthenticated.
    /// </summary>
    OrganizationId OrganizationId { get; }

    /// <summary>
    /// The caller's email address. Throws when unauthenticated.
    /// </summary>
    Email Email { get; }
}
