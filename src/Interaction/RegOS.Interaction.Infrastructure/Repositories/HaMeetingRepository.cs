using Microsoft.EntityFrameworkCore;

using RegOS.Interaction.Domain.Meetings;
using RegOS.Persistence;

namespace RegOS.Interaction.Infrastructure.Repositories;

public sealed class HaMeetingRepository : IHaMeetingRepository
{
    private readonly RegOSDbContext _dbContext;

    public HaMeetingRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(HaMeeting meeting, CancellationToken cancellationToken)
    {
        await _dbContext.HaMeetings.AddAsync(meeting, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<HaMeeting?> GetByIdAsync(
        HaMeetingId id, CancellationToken cancellationToken)
        => await _dbContext.HaMeetings
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task UpdateAsync(HaMeeting meeting, CancellationToken cancellationToken)
    {
        _dbContext.HaMeetings.Update(meeting);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
