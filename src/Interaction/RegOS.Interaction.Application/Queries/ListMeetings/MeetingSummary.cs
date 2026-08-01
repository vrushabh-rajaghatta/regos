namespace RegOS.Interaction.Application.Queries.ListMeetings;

public sealed record MeetingSummary(
    Guid MeetingId,
    string Subject,
    Guid AuthorityId,
    string AuthorityName,
    string? AuthorityDivisionName,
    DateOnly RaisedOn,
    DateOnly? ScheduledFor,
    DateOnly? HeldOn,
    string CurrentStatus,
    string? Minutes,
    string? Outcome,
    IReadOnlyList<MeetingHistoryEntry> History);

public sealed record MeetingHistoryEntry(
    string Status,
    DateOnly OccurredOn,
    DateTime RecordedOnUtc,
    string? Note);
