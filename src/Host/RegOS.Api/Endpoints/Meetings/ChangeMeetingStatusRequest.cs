namespace RegOS.Api.Endpoints.Meetings;

public sealed record ChangeMeetingStatusRequest(
    string Status,
    DateOnly OccurredOn,
    string? Note = null);
