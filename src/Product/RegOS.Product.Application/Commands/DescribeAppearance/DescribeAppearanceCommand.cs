using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.DescribeAppearance;

/// <summary>
/// Describes what a presentation looks like.
/// </summary>
/// <remarks>
/// Its own command rather than five more fields on <c>RestatePresentation</c>: a
/// presentation is recorded when its dose form is known and described when
/// somebody has seen it, which is routinely later and by somebody else.
/// </remarks>
/// <param name="ColourCodes">
/// Several is ordinary — a capsule with a white body and a blue cap is two
/// colours. Empty clears them.
/// </param>
/// <param name="Imprint">
/// What is stamped on it, and the one part of an appearance anybody looks a
/// medicine <em>up</em> by.
/// </param>
public sealed record DescribeAppearanceCommand(
    PharmaceuticalProductDetailId PresentationId,
    IReadOnlyList<string> ColourCodes,
    string? ShapeCode,
    string? Imprint,
    string? Description);
