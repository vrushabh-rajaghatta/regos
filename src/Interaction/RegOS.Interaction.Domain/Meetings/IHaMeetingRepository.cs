namespace RegOS.Interaction.Domain.Meetings;

public interface IHaMeetingRepository
{
    Task AddAsync(HaMeeting meeting, CancellationToken cancellationToken);

    Task<HaMeeting?> GetByIdAsync(HaMeetingId id, CancellationToken cancellationToken);

    Task UpdateAsync(HaMeeting meeting, CancellationToken cancellationToken);
}
