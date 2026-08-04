using RegOS.Product.Domain.Product;

namespace RegOS.Labeling.Application.Commands.CreateGlobalLabel;

/// <summary>
/// Starts holding a label for a product, above any market.
/// </summary>
/// <param name="LabelTypeCode">
/// A code from <c>LabelVocabulary.GlobalLabelTypes</c>, not a display name: the
/// wire carries the code so a re-worded label does not break a caller.
/// </param>
public sealed record CreateGlobalLabelCommand(
    GlobalProductId GlobalProductId,
    string Name,
    string LabelTypeCode);
