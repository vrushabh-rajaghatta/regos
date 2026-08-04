namespace RegOS.Labeling.Application.Queries.ListCoreVersionsForProduct;

/// <param name="LabelName">
/// Which core label this version belongs to — a product may hold a core data
/// sheet and a core safety information document, and "version 3" alone would be
/// ambiguous between them.
/// </param>
public sealed record CoreVersionOption(
    Guid Id,
    Guid GlobalLabelId,
    string LabelName,
    int VersionNumber,
    string Status,
    DateOnly? EffectiveFrom);
