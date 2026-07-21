using RegOS.Platform.Domain.Aggregates.Invitation;
using RegOS.Platform.Domain.Aggregates.User;

using InvitationAggregate =
    RegOS.Platform.Domain.Aggregates.Invitation.Invitation;

namespace RegOS.Platform.Application.Tests.Fakes;

/// <summary>In-memory stand-in that records what the handler persisted.</summary>
public sealed class FakeInvitationRepository : IInvitationRepository
{
    private readonly List<InvitationAggregate> _invitations = new();

    public FakeInvitationRepository(params InvitationAggregate[] existing)
    {
        _invitations.AddRange(existing);
    }

    public IReadOnlyList<InvitationAggregate> All => _invitations;

    public InvitationAggregate? Added { get; private set; }

    public List<InvitationAggregate> Updated { get; } = new();

    public Task AddAsync(
        InvitationAggregate invitation, CancellationToken cancellationToken)
    {
        Added = invitation;
        _invitations.Add(invitation);
        return Task.CompletedTask;
    }

    public Task<InvitationAggregate?> GetByTokenHashAsync(
        string tokenHash, CancellationToken cancellationToken)
        => Task.FromResult(
            _invitations.FirstOrDefault(x => x.TokenHash == tokenHash));

    public Task<IReadOnlyList<InvitationAggregate>> GetPendingForUserAsync(
        UserId userId, CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<InvitationAggregate>>(
            _invitations
                .Where(x => x.UserId == userId
                    && x.ConsumedOn is null
                    && x.RevokedOn is null)
                .ToList());

    public Task UpdateAsync(
        InvitationAggregate invitation, CancellationToken cancellationToken)
    {
        Updated.Add(invitation);
        return Task.CompletedTask;
    }
}
