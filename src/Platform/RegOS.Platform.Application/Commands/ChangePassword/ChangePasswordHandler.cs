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

        // A DomainException, so 400 — not 401, which this originally was.
        //
        // Named plainly, unlike sign-in's uniform message: the caller is
        // authenticated, so naming the fault reveals nothing about which
        // accounts exist. But the *status* matters as much as the message. 401
        // means "re-authenticate", and every well-behaved client acts on it —
        // ours refreshes the session and replays the request, then reports the
        // second 401 as a dead session. A browser spec caught it: mistyping
        // your current password signed you out of the application.
        //
        // The caller is authenticated (so not 401) and permitted to change
        // their own password (so not 403). What is wrong is a field in the
        // request, which is a 400.
        if (_passwordHasher.Verify(
                credential.PasswordHash, command.CurrentPassword ?? string.Empty)
            == PasswordVerification.Failed)
        {
            throw new DomainException(
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
