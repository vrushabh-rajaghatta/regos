using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Platform.Domain.Aggregates.Invitation;
using RegOS.Platform.Domain.Aggregates.User;

using InvitationAggregate =
    RegOS.Platform.Domain.Aggregates.Invitation.Invitation;

namespace RegOS.Platform.Infrastructure.Repositories;

public sealed class InvitationRepository : IInvitationRepository
{
    private readonly RegOSDbContext _dbContext;

    public InvitationRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        InvitationAggregate invitation, CancellationToken cancellationToken)
    {
        await _dbContext.Invitations.AddAsync(invitation, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<InvitationAggregate?> GetByTokenHashAsync(
        string tokenHash, CancellationToken cancellationToken)
        => await _dbContext.Invitations
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

    public async Task<IReadOnlyList<InvitationAggregate>> GetPendingForUserAsync(
        UserId userId, CancellationToken cancellationToken)
        // Filtered in SQL on the columns that define "pending" rather than by
        // calling IsPendingAt, which EF cannot translate (ADR-020). Expiry is
        // left out on purpose: an expired invitation still needs revoking so a
        // resend leaves exactly one live token behind.
        => await _dbContext.Invitations
            .Where(x => x.UserId == userId
                && x.ConsumedOn == null
                && x.RevokedOn == null)
            .ToListAsync(cancellationToken);

    public async Task UpdateAsync(
        InvitationAggregate invitation, CancellationToken cancellationToken)
    {
        _dbContext.Invitations.Update(invitation);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
