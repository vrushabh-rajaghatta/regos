namespace RegOS.Interaction.Application.Queries.ListInspections;

public sealed record InspectionSummary(
    Guid InspectionId,
    string Title,
    Guid AuthorityId,
    string AuthorityName,
    Guid? OrganizationSiteId,
    string? OrganizationSiteName,
    DateOnly RaisedOn,
    DateOnly? ScheduledFor,
    DateOnly? CompletedOn,
    string CurrentStatus,
    string? Outcome,
    IReadOnlyList<InspectionHistoryEntry> History);

public sealed record InspectionHistoryEntry(
    string Status,
    DateOnly OccurredOn,
    DateTime RecordedOnUtc,
    string? Note);
