using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Contracts;

using InvitationAggregate =
    RegOS.Platform.Domain.Aggregates.Invitation.Invitation;

namespace RegOS.Platform.Domain.Aggregates.Invitation;

public interface IInvitationRepository
{
    Task AddAsync(
        InvitationAggregate invitation,
        CancellationToken cancellationToken);

    /// <summary>
    /// The only way to find one, since the token value is never stored. Returns
    /// consumed, revoked and expired invitations too — the caller decides, and
    /// must be able to tell "no such invitation" from "one that is finished".
    /// </summary>
    Task<InvitationAggregate?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken);

    /// <summary>
    /// Every invitation for a user that has not been consumed or revoked. Used
    /// by resend, which retires the previous token so at most one is ever live.
    /// </summary>
    Task<IReadOnlyList<InvitationAggregate>> GetPendingForUserAsync(
        UserId userId,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        InvitationAggregate invitation,
        CancellationToken cancellationToken);
}
