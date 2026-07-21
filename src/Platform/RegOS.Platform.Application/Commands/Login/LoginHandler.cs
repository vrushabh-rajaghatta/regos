using RegOS.Platform.Application.Authentication;
using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.RefreshToken;
using RegOS.Platform.Domain.Aggregates.Session;
using RegOS.Platform.Domain.Aggregates.Tenant;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.Aggregates.UserCredential;
using RegOS.Platform.Domain.ValueObjects;
using RegOS.SharedKernel.Exceptions;

using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;

namespace RegOS.Platform.Application.Commands.Login;

/// <summary>
/// Exchanges an email and password for an access token.
///
/// Every failure leaves by the same door. Unknown email, wrong password,
/// inactive user, invited-but-not-activated user, user with no credential — all
/// raise <see cref="AuthenticationFailedException"/> with one message, so the
/// endpoint cannot be used to discover which accounts exist (ADR-022). Since
/// ADR-021 made addresses globally unique, that oracle would otherwise answer
/// for every organization at once.
/// </summary>
public sealed class LoginHandler
{
    /// <summary>
    /// A real hash of a value nobody knows, verified against when no credential
    /// was found. Without it, a missing account would answer measurably faster
    /// than a wrong password, and the timing alone would enumerate accounts.
    /// </summary>
    private readonly Lazy<string> _decoyHash;

    private readonly SessionFactory _sessions;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly ISessionRepository _sessionStore;
    private readonly IUserCredentialRepository _credentials;
    private readonly IUserRepository _users;
    private readonly ITenantRepository _tenants;

    public LoginHandler(
        SessionFactory sessions,
        IPasswordHasher passwordHasher,
        IRefreshTokenRepository refreshTokens,
        ISessionRepository sessionStore,
        IUserCredentialRepository credentials,
        IUserRepository users,
        ITenantRepository tenants)
    {
        _sessions = sessions;
        _passwordHasher = passwordHasher;
        _refreshTokens = refreshTokens;
        _sessionStore = sessionStore;
        _credentials = credentials;
        _users = users;
        _tenants = tenants;

        _decoyHash = new Lazy<string>(() => _passwordHasher.Hash(
            Password.Create(Guid.NewGuid().ToString())));
    }

    public async Task<AuthenticatedSession> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        var user = await FindUserAsync(command.Email, cancellationToken);

        var credential = user is null
            ? null
            : await _credentials.GetByUserIdAsync(user.Id, cancellationToken);

        // Verify unconditionally, even with nothing to verify against, so the
        // work done for an unknown account matches a known one.
        var verification = _passwordHasher.Verify(
            credential?.PasswordHash ?? _decoyHash.Value,
            command.Password ?? string.Empty);

        if (user is null
            || credential is null
            || verification == PasswordVerification.Failed)
        {
            throw new AuthenticationFailedException(
                AuthenticationErrors.InvalidCredentials);
        }

        // Status is checked only after the password is proven. Rejecting an
        // inactive account before verification would confirm the address exists
        // to anyone who guessed it.
        if (user.Status != UserStatus.Active)
        {
            throw new AuthenticationFailedException(
                AuthenticationErrors.InvalidCredentials);
        }

        // A retired tenant means "no one signs in" (Tenant.Deactivate), and
        // sign-in is where that is enforced. Platform users have no tenant
        // and skip this. Same message as every other rejection (ADR-022).
        if (user.TenantId is not null)
        {
            var tenant = await _tenants.GetByIdAsync(
                user.TenantId, cancellationToken);

            if (tenant is null || tenant.Status != TenantStatus.Active)
            {
                throw new AuthenticationFailedException(
                    AuthenticationErrors.InvalidCredentials);
            }
        }

        // The user has just proven they know the password, which is the only
        // moment the plaintext is available to rehash with current parameters.
        // Deferring this means it never happens.
        var now = DateTime.UtcNow;

        if (verification == PasswordVerification.SucceededNeedsRehash)
        {
            credential.ChangePassword(
                _passwordHasher.Hash(Password.Create(command.Password)),
                now);

            await _credentials.UpdateAsync(credential, cancellationToken);
        }

        var (tokens, session, record) = _sessions.Start(
            user, command.UserAgent, command.IpAddress, now);

        // Session first: a token pointing at a session that does not exist
        // would be invisible on the sessions page and impossible to revoke
        // from it.
        await _sessionStore.AddAsync(session, cancellationToken);

        await _refreshTokens.AddAsync(record, cancellationToken);

        return tokens;
    }

    /// <summary>
    /// A malformed address is treated as a failed sign-in rather than a bad
    /// request. At this endpoint every rejection should look identical; a 400
    /// here would tell a caller their input got as far as the lookup.
    /// </summary>
    private async Task<UserAggregate?> FindUserAsync(
        string? value,
        CancellationToken cancellationToken)
    {
        Email email;

        try
        {
            email = Email.Create(value);
        }
        catch (DomainException)
        {
            return null;
        }

        return await _users.GetByEmailAsync(email, cancellationToken);
    }
}
