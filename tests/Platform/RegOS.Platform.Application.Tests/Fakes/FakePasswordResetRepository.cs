using RegOS.Platform.Domain.Aggregates.PasswordReset;
using RegOS.Platform.Domain.Aggregates.User;

using PasswordResetAggregate =
    RegOS.Platform.Domain.Aggregates.PasswordReset.PasswordReset;

namespace RegOS.Platform.Application.Tests.Fakes;

/// <summary>In-memory stand-in that records what the handler persisted.</summary>
public sealed class FakePasswordResetRepository : IPasswordResetRepository
{
    private readonly List<PasswordResetAggregate> _resets = new();

    public FakePasswordResetRepository(params PasswordResetAggregate[] existing)
    {
        _resets.AddRange(existing);
    }

    public IReadOnlyList<PasswordResetAggregate> All => _resets;

    public PasswordResetAggregate? Added { get; private set; }

    public List<PasswordResetAggregate> Updated { get; } = new();

    public Task AddAsync(
        PasswordResetAggregate reset, CancellationToken cancellationToken)
    {
        Added = reset;
        _resets.Add(reset);
        return Task.CompletedTask;
    }

    public Task<PasswordResetAggregate?> GetByTokenHashAsync(
        string tokenHash, CancellationToken cancellationToken)
        => Task.FromResult(
            _resets.FirstOrDefault(x => x.TokenHash == tokenHash));

    public Task<IReadOnlyList<PasswordResetAggregate>> GetUsableForUserAsync(
        UserId userId, CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<PasswordResetAggregate>>(
            _resets
                .Where(x => x.UserId == userId
                    && x.ConsumedOn is null
                    && x.RevokedOn is null)
                .ToList());

    public Task UpdateAsync(
        PasswordResetAggregate reset, CancellationToken cancellationToken)
    {
        Updated.Add(reset);
        return Task.CompletedTask;
    }
}
