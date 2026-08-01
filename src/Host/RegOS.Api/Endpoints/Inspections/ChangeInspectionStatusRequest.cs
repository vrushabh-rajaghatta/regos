namespace RegOS.Api.Endpoints.Inspections;

public sealed record ChangeInspectionStatusRequest(
    string Status,
    DateOnly OccurredOn,
    string? Note = null);
