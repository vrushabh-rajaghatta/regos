using RegOS.Platform.Application.Commands.Login;
using RegOS.Platform.Application.Commands.SetUserPassword;
using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.Invitation;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Platform.Application.Commands.AcceptInvitation;

/// <summary>
/// Turns an invited user into an active one with a password.
/// </summary>
/// <remarks>
/// <para>
/// An orchestrator, and almost nothing else. Validating the token is the only
/// new behaviour here; setting a password and activating a user already existed
/// and are used unchanged (ADR-027).
/// </para>
/// <para>
/// Anonymous by necessity — the caller has no session, that is what they are
/// here to obtain — so nothing in this path may depend on
/// <c>ITenantContext</c>. <see cref="SetUserPasswordHandler"/> resolves users by
/// id rather than by tenant, which is what makes it reusable here;
/// <c>ActivateUserHandler</c> is tenant-scoped and is deliberately not used.
/// </para>
/// </remarks>
public sealed class AcceptInvitationHandler
{
    private readonly SetUserPasswordHandler _setPassword;
    private readonly IInvitationTokenIssuer _tokens;
    private readonly IInvitationRepository _invitations;
    private readonly IUserRepository _users;

    public AcceptInvitationHandler(
        SetUserPasswordHandler setPassword,
        IInvitationTokenIssuer tokens,
        IInvitationRepository invitations,
        IUserRepository users)
    {
        _setPassword = setPassword;
        _tokens = tokens;
        _invitations = invitations;
        _users = users;
    }

    public async Task HandleAsync(
        AcceptInvitationCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Token)) throw Rejected();

        var invitation = await _invitations.GetByTokenHashAsync(
            _tokens.Hash(command.Token), cancellationToken);

        var now = DateTime.UtcNow;

        // Unknown, expired, already used and withdrawn are one answer. A caller
        // holding a stale link must not learn which of those it is, and an
        // attacker guessing tokens must not learn that one existed (ADR-022).
        if (invitation is null || !invitation.IsPendingAt(now)) throw Rejected();

        var user = await _users.GetByIdAsync(invitation.UserId, cancellationToken);

        // Not Invited means the account was deactivated after the invitation
        // was sent, or has already been through this. Someone withdrew access
        // deliberately, so the invitation no longer represents the
        // organization's intent and cannot be honoured.
        if (user is null || user.Status != UserStatus.Invited) throw Rejected();

        // Password first. These are separate units of work, and if a later step
        // fails the user is left invited with a credential — which they can
        // retry — rather than active without one, the exact state this slice
        // exists to make unreachable.
        await _setPassword.HandleAsync(
            new SetUserPasswordCommand(user.Id, command.Password),
            cancellationToken);

        user.Activate();

        await _users.UpdateAsync(user, cancellationToken);

        // Last: until this succeeds the invitation is still pending, so a
        // failure anywhere above leaves a link that still works.
        invitation.Consume(now);

        await _invitations.UpdateAsync(invitation, cancellationToken);
    }

    /// <summary>
    /// One rejection for every reason, but a different message from sign-in's —
    /// deliberately, because the threat is not the same. Sign-in is addressed
    /// by an email a stranger can guess, so naming the fault would enumerate
    /// accounts. An invitation is addressed by 256 random bits, which nobody
    /// guesses, so saying "expired or already used" costs nothing and is the
    /// difference between a user who resends and a user who is stuck. It still
    /// does not say <em>which</em>.
    /// </summary>
    private static AuthenticationFailedException Rejected() =>
        new(AuthenticationErrors.InvalidInvitation);
}
