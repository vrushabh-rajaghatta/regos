using RegOS.Platform.Domain.Aggregates.User;

using PasswordResetAggregate =
    RegOS.Platform.Domain.Aggregates.PasswordReset.PasswordReset;

namespace RegOS.Platform.Domain.Aggregates.PasswordReset;

public interface IPasswordResetRepository
{
    Task AddAsync(
        PasswordResetAggregate reset,
        CancellationToken cancellationToken);

    /// <summary>
    /// Looks a reset up by its hash — the only way it can be found, since the
    /// value is never stored. Returns spent, withdrawn and expired resets too:
    /// the handler decides what to do with them, and it cannot decide if the
    /// repository has already filtered them away.
    /// </summary>
    Task<PasswordResetAggregate?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken);

    /// <summary>
    /// Every reset for this user that could still be redeemed. Requesting a new
    /// link withdraws these, so that at most one is live at a time.
    /// </summary>
    Task<IReadOnlyList<PasswordResetAggregate>> GetUsableForUserAsync(
        UserId userId,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        PasswordResetAggregate reset,
        CancellationToken cancellationToken);
}
