namespace RegOS.Api.Endpoints.Correspondence;

public sealed record ResolveQuestionRequest(
    DateOnly OccurredOn,
    string? Note = null);
