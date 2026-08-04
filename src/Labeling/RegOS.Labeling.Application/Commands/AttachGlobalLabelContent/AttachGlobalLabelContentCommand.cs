using RegOS.Labeling.Domain.Aggregates.GlobalLabels;
using RegOS.ProductDocument.Domain.IDs;

namespace RegOS.Labeling.Application.Commands.AttachGlobalLabelContent;

/// <summary>
/// Points a draft at the document it is. The file itself already exists in
/// <c>ProductDocument</c>; this records what it means (ADR-059 §6).
/// </summary>
public sealed record AttachGlobalLabelContentCommand(
    GlobalLabelId GlobalLabelId,
    GlobalLabelVersionId VersionId,
    ProductDocumentId ContentId);
