using RegOS.Platform.Application.PasswordResets;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Platform.Application.Commands.RequestPasswordReset;

/// <summary>
/// Sends a reset link, if there is anybody to send one to.
/// </summary>
/// <remarks>
/// <para>
/// Succeeds no matter what. A malformed address, an address nobody has, an
/// account still waiting to accept its invitation, a deactivated account — all
/// finish the same way, because this endpoint is anonymous and anything that
/// distinguished them would answer "does this person have an account here?"
/// for any address a stranger cares to try (ADR-022).
/// </para>
/// <para>
/// Invited users are deliberately among the silent cases. A reset recovers a
/// credential that exists; it does not create a first one. Letting it do so
/// would open a second route to a user's first password and undo the single
/// route AUTH-007 established (ADR-027). Someone in that state needs their
/// invitation resent, which is an administrator's action.
/// </para>
/// </remarks>
public sealed class RequestPasswordResetHandler
{
    private readonly PasswordResetIssuer _resets;
    private readonly IUserRepository _users;

    public RequestPasswordResetHandler(
        PasswordResetIssuer resets,
        IUserRepository users)
    {
        _resets = resets;
        _users = users;
    }

    public async Task HandleAsync(
        RequestPasswordResetCommand command,
        CancellationToken cancellationToken)
    {
        if (!TryParse(command.Email, out var email)) return;

        var user = await _users.GetByEmailAsync(email, cancellationToken);

        if (user is null || user.Status != UserStatus.Active) return;

        await _resets.IssueAsync(user, DateTime.UtcNow, cancellationToken);
    }

    /// <summary>
    /// A malformed address is not a bad request here. Returning 400 for one and
    /// 204 for another would let a caller tell a well-formed unknown address
    /// from a malformed one, which is a small oracle but still an oracle.
    /// </summary>
    private static bool TryParse(string? value, out Email email)
    {
        try
        {
            email = Email.Create(value);

            return true;
        }
        catch (DomainException)
        {
            email = default!;

            return false;
        }
    }
}
