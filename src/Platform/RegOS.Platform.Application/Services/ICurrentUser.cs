using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;
using RegOS.SharedKernel.Primitives;
using RegOS.Platform.Contracts;

namespace RegOS.Platform.Application.Services;

/// <summary>
/// The authenticated caller behind the current request.
/// </summary>
/// <remarks>
/// <para>
/// Deliberately five members and no more. The pressure on a type like this is
/// always to grow — a display name here, the organization's name there,
/// because a page needs it — until every service depends on it and none of
/// them can say why. Everything absent from this interface can be resolved
/// from <see cref="UserId"/> by whoever actually needs it.
/// </para>
/// <para>
/// <see cref="Role"/> arrived with ADR-033, in the minimal three-value shape
/// the epic needed — not the permission matrix this doc once warned about
/// guessing at. Permissions beyond the role stay absent until decided.
/// </para>
/// <para>
/// This is a sibling of <c>ITenantContext</c>, not a replacement. That answers
/// <em>which tenant is this request scoped to</em>, which is an infrastructure
/// question every context asks; this answers <em>which person is calling</em>,
/// which is a Platform concept and typed accordingly. They agree today because
/// a user belongs to exactly one tenant (ADR-030).
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
    /// The tenant the caller belongs to. Throws when unauthenticated.
    /// </summary>
    TenantId TenantId { get; }

    /// <summary>
    /// The caller's email address. Throws when unauthenticated.
    /// </summary>
    Email Email { get; }

    /// <summary>
    /// The caller's role, as their token states it (ADR-033). Throws when
    /// unauthenticated. Endpoint gating uses the authorization policies, not
    /// this — this exists for the rare handler and for /me.
    /// </summary>
    UserRole Role { get; }
}
