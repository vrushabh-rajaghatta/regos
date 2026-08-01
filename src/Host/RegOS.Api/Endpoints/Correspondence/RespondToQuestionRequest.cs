namespace RegOS.Api.Endpoints.Correspondence;

public sealed record RespondToQuestionRequest(
    string ResponseText,
    DateOnly OccurredOn,
    string? Note = null);
