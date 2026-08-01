using RegOS.Platform.Domain.Aggregates.User;

using SessionAggregate = RegOS.Platform.Domain.Aggregates.Session.Session;
using RegOS.Platform.Contracts;

namespace RegOS.Platform.Domain.Aggregates.Session;

public interface ISessionRepository
{
    Task AddAsync(SessionAggregate session, CancellationToken cancellationToken);

    Task<SessionAggregate?> GetByIdAsync(
        SessionId id, CancellationToken cancellationToken);

    /// <summary>
    /// Every session the user could still be signed in on. What the sessions
    /// list shows, and what "sign out everywhere" acts on.
    /// </summary>
    Task<IReadOnlyList<SessionAggregate>> GetActiveForUserAsync(
        UserId userId, CancellationToken cancellationToken);

    Task UpdateAsync(SessionAggregate session, CancellationToken cancellationToken);
}
