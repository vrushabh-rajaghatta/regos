using RegOS.Platform.Application.Authentication;
using RegOS.Platform.Application.Commands.Login;
using RegOS.Platform.Application.Commands.SetUserPassword;
using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.Aggregates.UserCredential;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Platform.Application.Commands.ChangePassword;

/// <summary>
/// Replaces the password of the signed-in user, who proves entitlement by
/// knowing the current one.
/// </summary>
/// <remarks>
/// <para>
/// The third caller of <see cref="SetUserPasswordHandler"/>, and the third
/// distinct proof of entitlement: acceptance proves possession of an
/// invitation, reset proves possession of a mailbox, and this proves knowledge
/// of the existing secret. The primitive itself knows none of that — it
/// replaces a credential and nothing else.
/// </para>
/// <para>
/// Unlike its two siblings this one requires a session, so it is the only
/// credential flow that can read its user from <c>ICurrentUser</c>.
/// </para>
/// </remarks>
public sealed class ChangePasswordHandler
{
    private readonly SetUserPasswordHandler _setPassword;
    private readonly CredentialTrustRevoker _revoker;
    private readonly ICurrentUser _currentUser;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserCredentialRepository _credentials;
    private readonly IUserRepository _users;

    public ChangePasswordHandler(
        SetUserPasswordHandler setPassword,
        CredentialTrustRevoker revoker,
        ICurrentUser currentUser,
        IPasswordHasher passwordHasher,
        IUserCredentialRepository credentials,
        IUserRepository users)
    {
        _setPassword = setPassword;
        _revoker = revoker;
        _currentUser = currentUser;
        _passwordHasher = passwordHasher;
        _credentials = credentials;
        _users = users;
    }

    public async Task HandleAsync(
        ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var user = await _users.GetByIdAsync(userId, cancellationToken);

        // An access token outlives a deactivation by up to its fifteen minutes,
        // so being authenticated is not the same as being allowed. Someone
        // whose access was withdrawn a moment ago must not be able to set a new
        // password on the way out.
        if (user is null || user.Status != UserStatus.Active)
        {
            throw new AuthenticationFailedException(
                AuthenticationErrors.InvalidCredentials);
        }

        var credential = await _credentials.GetByUserIdAsync(
            userId, cancellationToken);

        if (credential is null)
        {
            throw new AuthenticationFailedException(
                AuthenticationErrors.InvalidCredentials);
        }

        // Named plainly, unlike sign-in's uniform message. There is no
        // enumeration risk to defend against: the caller is authenticated, so
        // the answer tells them nothing they did not already know about which
        // accounts exist — and "the current password is incorrect" is the
        // difference between a user who retries and one who is baffled.
        if (_passwordHasher.Verify(
                credential.PasswordHash, command.CurrentPassword ?? string.Empty)
            == PasswordVerification.Failed)
        {
            throw new AuthenticationFailedException(
                AuthenticationErrors.IncorrectCurrentPassword);
        }

        // No "must differ from the current password" rule. Password validity is
        // defined by the Password value object, and reuse policy - none, last
        // N, minimum age - is a product feature, not something to invent here.
        await _setPassword.HandleAsync(
            new SetUserPasswordCommand(userId, command.NewPassword),
            cancellationToken);

        // Including the session making this very request. Keeping it alive
        // would mean knowing which refresh token is the current one, which
        // means threading session transport into an authenticated command;
        // "sign out my other devices" belongs to AUTH-010, which will have the
        // vocabulary for it.
        await _revoker.RevokeEverythingDerivedFromTheOldPasswordAsync(
            userId, DateTime.UtcNow, cancellationToken);
    }
}
