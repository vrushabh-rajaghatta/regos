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

/// <summary>
/// What the medicine looks like.
/// </summary>
/// <param name="ColourCodes">
/// Several is ordinary — a capsule with a white body and a blue cap is two
/// colours. Absent or empty clears them.
/// </param>
/// <param name="Imprint">
/// What is stamped on it. Its own field rather than a phrase in
/// <paramref name="Description"/>, because it is the one part of an appearance
/// anybody looks a medicine <em>up</em> by.
/// </param>
public sealed record DescribeAppearanceRequest(
    IReadOnlyList<string>? ColourCodes,
    string? ShapeCode,
    string? Imprint,
    string? Description);
