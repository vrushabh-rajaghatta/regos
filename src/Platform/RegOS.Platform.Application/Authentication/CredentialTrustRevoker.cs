using RegOS.Platform.Domain.Aggregates.PasswordReset;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Contracts;

namespace RegOS.Platform.Application.Authentication;

/// <summary>
/// Ends everything that was trusted on the strength of a password, because that
/// password has just been replaced.
/// </summary>
/// <remarks>
/// <para>
/// The whole of ADR-028 in one place. When a credential is replaced, two things
/// derived from the old one must stop working:
/// </para>
/// <list type="bullet">
/// <item>every live session, because it was opened by proving the old password;</item>
/// <item>every outstanding password reset grant, because it is an alternative
/// way of replacing the credential that somebody else may be holding.</item>
/// </list>
/// <para>
/// The second is the one that is easy to miss, and it is the reason this class
/// exists rather than two calls at each site. A user who changes their password
/// believes they have shut out whoever prompted them to; a live reset link in a
/// mailbox someone else can read means they have not.
/// </para>
/// <para>
/// Invitations are deliberately untouched. An invitation establishes a
/// <em>first</em> credential and cannot exist for a user who has one, so there
/// is nothing here for it to invalidate (ADR-027).
/// </para>
/// <para>
/// Two callers, not three, and extracted anyway — the same judgement as
/// <c>InvitationIssuer</c>. ADR-018 tolerates duplication that costs a little
/// tidiness; it does not tolerate duplication where forgetting half of it is a
/// silent security hole.
/// </para>
/// </remarks>
public sealed class CredentialTrustRevoker
{
    private readonly SessionRevoker _sessions;
    private readonly IPasswordResetRepository _resets;

    public CredentialTrustRevoker(
        SessionRevoker sessions,
        IPasswordResetRepository resets)
    {
        _sessions = sessions;
        _resets = resets;
    }

    public async Task RevokeEverythingDerivedFromTheOldPasswordAsync(
        UserId userId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        await _sessions.RevokeEveryFor(userId, now, cancellationToken);

        foreach (var outstanding in await _resets.GetUsableForUserAsync(
            userId, cancellationToken))
        {
            outstanding.Revoke(now);

            await _resets.UpdateAsync(outstanding, cancellationToken);
        }
    }
}
