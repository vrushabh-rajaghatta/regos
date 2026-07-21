using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.PasswordReset;

using PasswordResetAggregate =
    RegOS.Platform.Domain.Aggregates.PasswordReset.PasswordReset;
using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;

namespace RegOS.Platform.Application.PasswordResets;

/// <summary>
/// Issues a password reset for a user and tells them about it.
/// </summary>
/// <remarks>
/// The same shape as <c>InvitationIssuer</c>, and for the same reason it was
/// extracted there: "revoke what is outstanding, then issue, then notify" is a
/// sequence whose steps must not drift apart, because forgetting the first is
/// how two live reset links end up in one mailbox. Today it has one caller;
/// AUTH-009 may give it a second.
/// </remarks>
public sealed class PasswordResetIssuer
{
    private readonly IPasswordResetNotifier _notifier;
    private readonly IPasswordResetTokenIssuer _tokens;
    private readonly IPasswordResetRepository _resets;

    public PasswordResetIssuer(
        IPasswordResetNotifier notifier,
        IPasswordResetTokenIssuer tokens,
        IPasswordResetRepository resets)
    {
        _notifier = notifier;
        _tokens = tokens;
        _resets = resets;
    }

    public async Task IssueAsync(
        UserAggregate user,
        DateTime now,
        CancellationToken cancellationToken)
    {
        // Retire anything still outstanding first. Asking twice must invalidate
        // the first link, or a user who requested a reset, waited, and asked
        // again leaves two working links behind — and only knows about one.
        foreach (var outstanding in await _resets.GetUsableForUserAsync(
            user.Id, cancellationToken))
        {
            outstanding.Revoke(now);

            await _resets.UpdateAsync(outstanding, cancellationToken);
        }

        var token = _tokens.Issue(now);

        await _resets.AddAsync(
            PasswordResetAggregate.Issue(
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
