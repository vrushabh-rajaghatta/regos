using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.Aggregates.UserCredential;
using RegOS.Platform.Domain.ValueObjects;
using RegOS.SharedKernel.Exceptions;

using UserCredentialAggregate =
    RegOS.Platform.Domain.Aggregates.UserCredential.UserCredential;

namespace RegOS.Platform.Application.Commands.SetUserPassword;

/// <summary>
/// Sets or replaces a user's password. Deliberately not reachable over HTTP
/// yet: this slice establishes the primitive, and the flows that expose it —
/// invitation acceptance, password reset, change password — each carry their own
/// authorization rules and arrive with the login pipeline.
/// </summary>
public sealed class SetUserPasswordHandler
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserCredentialRepository _credentials;
    private readonly IUserRepository _users;

    public SetUserPasswordHandler(
        IPasswordHasher passwordHasher,
        IUserCredentialRepository credentials,
        IUserRepository users)
    {
        _passwordHasher = passwordHasher;
        _credentials = credentials;
        _users = users;
    }

    public async Task HandleAsync(
        SetUserPasswordCommand command,
        CancellationToken cancellationToken)
    {
        // Validate before touching the database: a password that cannot be
        // accepted should not cost a lookup, and the rule is decidable from the
        // request alone (ADR-009).
        var password = Password.Create(command.Password);

        var user = await _users.GetByIdAsync(command.UserId, cancellationToken);

        if (user is null)
            throw new NotFoundException(PlatformErrors.UserNotFound);

        var hash = _passwordHasher.Hash(password);

        var existing = await _credentials.GetByUserIdAsync(
            command.UserId,
            cancellationToken);

        var now = DateTime.UtcNow;

        if (existing is null)
        {
            await _credentials.AddAsync(
                UserCredentialAggregate.Create(command.UserId, hash, now),
                cancellationToken);

            return;
        }

        existing.ChangePassword(hash, now);

        await _credentials.UpdateAsync(existing, cancellationToken);
    }
}
