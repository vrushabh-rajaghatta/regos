using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.Invitation;

using InvitationAggregate =
    RegOS.Platform.Domain.Aggregates.Invitation.Invitation;
using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;

namespace RegOS.Platform.Application.Invitations;

/// <summary>
/// Issues an invitation for a user and tells them about it.
/// </summary>
/// <remarks>
/// One class rather than the same sequence in both <c>InviteUserHandler</c> and
/// <c>ResendInvitationHandler</c>. Like <c>SessionFactory</c>, this is not the
/// duplication ADR-018 tolerates twice: two implementations of "issue an
/// invitation" is how a resend path quietly stops revoking the previous token,
/// leaving two live at once. At most one invitation is ever pending per user,
/// and this is what guarantees it.
/// </remarks>
public sealed class InvitationIssuer
{
    private readonly IInvitationNotifier _notifier;
    private readonly IInvitationTokenIssuer _tokens;
    private readonly IInvitationRepository _invitations;

    public InvitationIssuer(
        IInvitationNotifier notifier,
        IInvitationTokenIssuer tokens,
        IInvitationRepository invitations)
    {
        _notifier = notifier;
        _tokens = tokens;
        _invitations = invitations;
    }

    public async Task IssueAsync(
        UserAggregate user,
        DateTime now,
        CancellationToken cancellationToken)
    {
        // Retire anything still outstanding first. A resend must invalidate the
        // link it replaces, or an old email keeps working alongside the new one.
        foreach (var pending in await _invitations.GetPendingForUserAsync(
            user.Id, cancellationToken))
        {
            pending.Revoke(now);

            await _invitations.UpdateAsync(pending, cancellationToken);
        }

        var token = _tokens.Issue(now);

        await _invitations.AddAsync(
            InvitationAggregate.Issue(
                user.Id, token.Hash, token.ExpiresAt, now),
            cancellationToken);

        // Last, and with the plaintext that exists only in this scope. Sending
        // before persisting would risk a link that matches nothing.
        await _notifier.SendAsync(
            user.Email,
            user.FirstName,
            token.Value,
            token.ExpiresAt,
            cancellationToken);
    }
}
