using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Platform.Domain.Aggregates.RefreshToken;
using RegOS.Platform.Domain.Aggregates.User;

using RefreshTokenAggregate =
    RegOS.Platform.Domain.Aggregates.RefreshToken.RefreshToken;

namespace RegOS.Platform.Infrastructure.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly RegOSDbContext _dbContext;

    public RefreshTokenRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        RefreshTokenAggregate token,
        CancellationToken cancellationToken)
    {
        await _dbContext.RefreshTokens.AddAsync(token, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<RefreshTokenAggregate?> GetByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken)
        => await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

    public async Task<IReadOnlyList<RefreshTokenAggregate>> GetActiveForUserAsync(
        UserId userId,
        CancellationToken cancellationToken)
        // Filtered in SQL on the two columns that define "active" rather than
        // by calling IsActiveAt, which EF cannot translate (ADR-020).
        => await _dbContext.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedOn == null)
            .ToListAsync(cancellationToken);

    public async Task UpdateAsync(
        RefreshTokenAggregate token,
        CancellationToken cancellationToken)
    {
        _dbContext.RefreshTokens.Update(token);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RotateAsync(
        RefreshTokenAggregate revoked,
        RefreshTokenAggregate issued,
        CancellationToken cancellationToken)
    {
        _dbContext.RefreshTokens.Update(revoked);
        await _dbContext.RefreshTokens.AddAsync(issued, cancellationToken);

        // One SaveChanges, therefore one transaction. Revoking the old token
        // and inserting the new one either both happen or neither does.
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
