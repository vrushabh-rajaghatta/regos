using RegOS.Platform.Domain.Aggregates.User;

using RefreshTokenAggregate =
    RegOS.Platform.Domain.Aggregates.RefreshToken.RefreshToken;

namespace RegOS.Platform.Domain.Aggregates.RefreshToken;

public interface IRefreshTokenRepository
{
    Task AddAsync(
        RefreshTokenAggregate token,
        CancellationToken cancellationToken);

    /// <summary>
    /// Looks a token up by its hash — the only way it can be found, since the
    /// value is never stored. Returns revoked and expired tokens too: the
    /// caller must be able to tell "no such token" from "a token that is no
    /// longer valid", because the second is evidence of replay.
    /// </summary>
    Task<RefreshTokenAggregate?> GetByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<RefreshTokenAggregate>> GetActiveForUserAsync(
        UserId userId,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        RefreshTokenAggregate token,
        CancellationToken cancellationToken);

    /// <summary>
    /// Persists a rotation as one unit. The old token's revocation and the new
    /// token's insertion must not be separable, or a crash between them either
    /// leaves two live tokens or none.
    /// </summary>
    Task RotateAsync(
        RefreshTokenAggregate revoked,
        RefreshTokenAggregate issued,
        CancellationToken cancellationToken);
}
