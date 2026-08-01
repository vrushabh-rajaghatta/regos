using RegOS.Platform.Application.Authentication;
using RegOS.Platform.Application.Commands.Login;
using RegOS.Platform.Application.Commands.SetUserPassword;
using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.PasswordReset;
using RegOS.Platform.Domain.Aggregates.RefreshToken;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.SharedKernel.Exceptions;
using RegOS.Platform.Contracts;

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
    private readonly CredentialTrustRevoker _revoker;
    private readonly IPasswordResetTokenIssuer _tokens;
    private readonly IPasswordResetRepository _resets;
    private readonly IUserRepository _users;

    public CompletePasswordResetHandler(
        SetUserPasswordHandler setPassword,
        CredentialTrustRevoker revoker,
        IPasswordResetTokenIssuer tokens,
        IPasswordResetRepository resets,
        IUserRepository users)
    {
        _setPassword = setPassword;
        _revoker = revoker;
        _tokens = tokens;
        _resets = resets;
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

        // Consumed before the revocation sweep, so that this grant is already
        // spent and the sweep finds only the *other* outstanding ones.
        reset.Consume(now);

        await _resets.UpdateAsync(reset, cancellationToken);

        // Whoever forced the reset — the user, or someone who had taken the
        // account — nothing established with the old password survives it
        // (ADR-028).
        await _revoker.RevokeEverythingDerivedFromTheOldPasswordAsync(
            user.Id, now, cancellationToken);
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
