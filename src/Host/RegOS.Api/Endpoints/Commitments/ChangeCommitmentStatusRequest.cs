namespace RegOS.Api.Endpoints.Commitments;

public sealed record ChangeCommitmentStatusRequest(
    string Status,
    DateOnly OccurredOn,
    string? Note = null);
