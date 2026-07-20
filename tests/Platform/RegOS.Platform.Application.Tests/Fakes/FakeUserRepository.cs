using RegOS.Platform.Domain.Aggregates.User;

using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;

namespace RegOS.Platform.Application.Tests.Fakes;

/// <summary>In-memory stand-in that records what the handler persisted.</summary>
public sealed class FakeUserRepository : IUserRepository
{
    private readonly UserAggregate? _existing;

    public FakeUserRepository(UserAggregate? existing = null)
    {
        _existing = existing;
    }

    public UserAggregate? Added { get; private set; }

    public UserAggregate? Updated { get; private set; }

    public Task AddAsync(UserAggregate user, CancellationToken cancellationToken)
    {
        Added = user;
        return Task.CompletedTask;
    }

    public Task<UserAggregate?> GetByIdAsync(
        UserId id,
        CancellationToken cancellationToken)
        => Task.FromResult(
            _existing is not null && _existing.Id == id ? _existing : null);

    public Task UpdateAsync(UserAggregate user, CancellationToken cancellationToken)
    {
        Updated = user;
        return Task.CompletedTask;
    }
}
