namespace RegOS.Api.Endpoints.Presentations;

/// <remarks>
/// One record for both add and restate, because the two take the same five
/// facts. It is a wire shape rather than a domain type, so sharing it says
/// nothing about the model — and a presentation that could be restated into a
/// state it could not be created in would be a gap, not a feature.
/// </remarks>
public sealed record PresentationRequest(
    string Name,
    string? Description,
    string DoseFormCode,
    string? UnitOfPresentationCode,
    IReadOnlyList<string>? RouteCodes);
