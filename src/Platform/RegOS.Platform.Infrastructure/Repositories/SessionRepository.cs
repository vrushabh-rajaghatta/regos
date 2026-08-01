using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Platform.Domain.Aggregates.Session;
using RegOS.Platform.Domain.Aggregates.User;

using SessionAggregate = RegOS.Platform.Domain.Aggregates.Session.Session;
using RegOS.Platform.Contracts;

namespace RegOS.Platform.Infrastructure.Repositories;

public sealed class SessionRepository : ISessionRepository
{
    private readonly RegOSDbContext _dbContext;

    public SessionRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        SessionAggregate session, CancellationToken cancellationToken)
    {
        await _dbContext.Sessions.AddAsync(session, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<SessionAggregate?> GetByIdAsync(
        SessionId id, CancellationToken cancellationToken)
        => await _dbContext.Sessions
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<SessionAggregate>> GetActiveForUserAsync(
        UserId userId, CancellationToken cancellationToken)
        // Filtered on the columns rather than by calling IsActiveAt, which EF
        // cannot translate (ADR-020). Expired sessions are excluded here
        // because this drives both the sessions list and "sign out everywhere",
        // and neither has anything to say about a session already over.
        => await _dbContext.Sessions
            .Where(x => x.UserId == userId
                && x.RevokedOn == null
                && x.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(x => x.LastUsedOn)
            .ToListAsync(cancellationToken);

    public async Task UpdateAsync(
        SessionAggregate session, CancellationToken cancellationToken)
    {
        _dbContext.Sessions.Update(session);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
