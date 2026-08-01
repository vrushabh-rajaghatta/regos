using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Platform.Domain.Aggregates.PasswordReset;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Contracts;

using PasswordResetAggregate =
    RegOS.Platform.Domain.Aggregates.PasswordReset.PasswordReset;

namespace RegOS.Platform.Infrastructure.Repositories;

public sealed class PasswordResetRepository : IPasswordResetRepository
{
    private readonly RegOSDbContext _dbContext;

    public PasswordResetRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        PasswordResetAggregate reset, CancellationToken cancellationToken)
    {
        await _dbContext.PasswordResets.AddAsync(reset, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PasswordResetAggregate?> GetByTokenHashAsync(
        string tokenHash, CancellationToken cancellationToken)
        => await _dbContext.PasswordResets
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

    public async Task<IReadOnlyList<PasswordResetAggregate>> GetUsableForUserAsync(
        UserId userId, CancellationToken cancellationToken)
        // Filtered in SQL on the columns rather than by calling IsUsableAt,
        // which EF cannot translate (ADR-020). Expiry is left out on purpose:
        // an expired reset still needs revoking so that requesting a new link
        // leaves exactly one live token behind.
        => await _dbContext.PasswordResets
            .Where(x => x.UserId == userId
                && x.ConsumedOn == null
                && x.RevokedOn == null)
            .ToListAsync(cancellationToken);

    public async Task UpdateAsync(
        PasswordResetAggregate reset, CancellationToken cancellationToken)
    {
        _dbContext.PasswordResets.Update(reset);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
