using RegOS.Platform.Application.Commands.Login;
using RegOS.Platform.Application.Commands.SetUserPassword;
using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.PasswordReset;
using RegOS.Platform.Domain.Aggregates.RefreshToken;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Platform.Application.Commands.CompletePasswordReset;

/// <summary>
/// Replaces a forgotten password and ends every session that used the old one.
/// </summary>
/// <remarks>
/// <para>
/// An orchestrator, like <c>AcceptInvitationHandler</c>. Validating the token
/// and revoking sessions are the only new behaviour; setting a password already
/// existed and is used unchanged.
/// </para>
/// <para>
/// Anonymous by necessity — someone who has forgotten their password has no
/// session — so nothing in this path may depend on <c>ITenantContext</c>.
/// <see cref="SetUserPasswordHandler"/> resolves users by id rather than by
/// tenant, which is what makes it reusable here.
/// </para>
/// <para>
/// No session is issued on success. Holding a reset link proves control of a
/// mailbox, not knowledge of the password just chosen, so the user signs in
/// afterwards through the one path that issues sessions — the same reasoning
/// that keeps acceptance from logging anybody in.
/// </para>
/// </remarks>
public sealed class CompletePasswordResetHandler
{
    private readonly SetUserPasswordHandler _setPassword;
    private readonly IPasswordResetTokenIssuer _tokens;
    private readonly IPasswordResetRepository _resets;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUserRepository _users;

    public CompletePasswordResetHandler(
        SetUserPasswordHandler setPassword,
        IPasswordResetTokenIssuer tokens,
        IPasswordResetRepository resets,
        IRefreshTokenRepository refreshTokens,
        IUserRepository users)
    {
        _setPassword = setPassword;
        _tokens = tokens;
        _resets = resets;
        _refreshTokens = refreshTokens;
        _users = users;
    }

    public async Task HandleAsync(
        CompletePasswordResetCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Token)) throw Rejected();

        var reset = await _resets.GetByTokenHashAsync(
            _tokens.Hash(command.Token), cancellationToken);

        var now = DateTime.UtcNow;

        // Unknown, expired, already used and withdrawn are one answer, for the
        // same reason acceptance gives one answer (ADR-022).
        if (reset is null || !reset.IsUsableAt(now)) throw Rejected();

        var user = await _users.GetByIdAsync(reset.UserId, cancellationToken);

        // Deactivated since the link was sent. Someone withdrew access
        // deliberately and a mailbox must not restore it.
        if (user is null || user.Status != UserStatus.Active) throw Rejected();

        // Password first, so that a failure below leaves a user who can sign in
        // with their new password and a link that still works, rather than one
        // who can do neither.
        await _setPassword.HandleAsync(
            new SetUserPasswordCommand(user.Id, command.Password),
            cancellationToken);

        reset.Consume(now);

        await _resets.UpdateAsync(reset, cancellationToken);

        await RevokeEverySessionForAsync(user.Id, now, cancellationToken);
    }

    /// <summary>
    /// Ends every live session. Whoever forced the reset — the user, or someone
    /// who had taken the account — the sessions opened before it can no longer
    /// be assumed to belong to the rightful owner.
    /// </summary>
    /// <remarks>
    /// Second occurrence of this loop; <c>RefreshSessionHandler</c> has the
    /// first. Intentionally duplicated pending AUTH-009, which will make it the
    /// third and give extraction real evidence rather than anticipation
    /// (ADR-018).
    /// </remarks>
    private async Task RevokeEverySessionForAsync(
        UserId userId, DateTime now, CancellationToken cancellationToken)
    {
        foreach (var token in await _refreshTokens.GetActiveForUserAsync(
            userId, cancellationToken))
        {
            token.Revoke(now);

            await _refreshTokens.UpdateAsync(token, cancellationToken);
        }
    }

    /// <summary>
    /// One rejection for every reason. Like acceptance, it names the likely
    /// causes without saying which applies: a 256-bit token cannot be
    /// enumerated, so the honesty costs nothing and saves a user who would
    /// otherwise be stuck on a dead link.
    /// </summary>
    private static AuthenticationFailedException Rejected() =>
        new(AuthenticationErrors.InvalidPasswordReset);
}
