namespace RegOS.Api.Endpoints.Meetings;

/// <param name="InitialStatus">
/// <c>Requested</c> when we asked for it, <c>Granted</c> when the authority
/// called it. Two different business events, so two different beginnings.
/// </param>
public sealed record BeginMeetingRequest(
    Guid AuthorityId,
    string Subject,
    string InitialStatus,
    DateOnly OccurredOn,
    DateOnly? ScheduledFor = null,
    Guid? AuthorityDivisionId = null,
    Guid? RegulatoryApplicationId = null);
