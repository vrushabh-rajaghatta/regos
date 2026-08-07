using Microsoft.EntityFrameworkCore;

using RegOS.Interaction.Domain.Meetings;
using RegOS.Persistence;

namespace RegOS.Interaction.Application.Queries.ListMeetings;

public sealed class ListMeetingsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListMeetingsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MeetingSummary>> HandleAsync(
        ListMeetingsQuery query,
        CancellationToken cancellationToken)
    {
        var meetings = _dbContext.HaMeetings.AsNoTracking();

        if (!query.IncludeConcluded)
        {
            meetings = meetings.Where(x =>
                x.CurrentStatus == HaMeetingStatus.Requested
                || x.CurrentStatus == HaMeetingStatus.Granted);
        }

        var divisions = await _dbContext.AuthorityDivisions
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var rows = await meetings
            .OrderBy(x => x.ScheduledFor == null)
            .ThenBy(x => x.ScheduledFor)
            .ThenBy(x => x.Id)
            .Join(
                _dbContext.Authorities.AsNoTracking(),
                x => x.AuthorityId,
                a => a.Id,
                (x, a) => new { Meeting = x, Authority = a })
            // BUG-001. The history is an OWNED collection — always loaded, and
            // no Include applies to it — so its order is settled here, in SQL,
            // where an entry id translates.
            .Select(x => new
            {
                x.Meeting,
                x.Authority,
                // Deterministic: an entry id is unique, so this is a
                // total order.
                History = x.Meeting.History.OrderBy(h => h.Id).ToList()
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new MeetingSummary(
                x.Meeting.Id.Value,
                x.Meeting.Subject,
                x.Authority.Id.Value,
                x.Authority.Name,
                x.Meeting.AuthorityDivisionId is { } d
                    ? divisions.GetValueOrDefault(d)
                    : null,
                x.Meeting.RaisedOn,
                x.Meeting.ScheduledFor,
                x.Meeting.HeldOn,
                x.Meeting.CurrentStatus.ToString(),
                x.Meeting.Minutes,
                x.Meeting.Outcome,
                // Deterministic: ordered by entry id in SQL above, and this
                // sort is stable (BUG-001).
                x.History
                    .OrderBy(h => h.OccurredOn)
                    .ThenBy(h => h.RecordedOnUtc)
                    .Select(h => new MeetingHistoryEntry(
                        h.Status.ToString(),
                        h.OccurredOn,
                        h.RecordedOnUtc,
                        h.Note))
                    .ToList()))
            .ToList();
    }
}
