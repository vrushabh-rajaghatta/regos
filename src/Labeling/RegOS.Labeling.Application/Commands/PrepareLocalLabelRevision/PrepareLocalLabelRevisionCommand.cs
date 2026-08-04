using RegOS.Labeling.Domain.Aggregates.GlobalLabels;
using RegOS.Labeling.Domain.Aggregates.LocalLabels;
using RegOS.ProductDocument.Domain.IDs;

namespace RegOS.Labeling.Application.Commands.PrepareLocalLabelRevision;

/// <summary>
/// Records everything settled while a revision is being prepared, before the
/// authority approves it.
/// </summary>
/// <remarks>
/// One command rather than four, and it <b>restates</b> rather than patches:
/// these facts are decided together, and a caller able to change the document
/// without the derivation could point a translation of core v7 at a file that
/// says v8.
/// </remarks>
/// <param name="DerivedFromGlobalLabelVersionId">
/// Null is legitimate — a migrated portfolio does not know, and a local-first
/// company holds approved labelling before any core label exists here (D3).
/// </param>
/// <param name="DataCarrierCode">
/// Artwork's one identifying attribute. Null on every other type.
/// </param>
public sealed record PrepareLocalLabelRevisionCommand(
    LocalLabelId LocalLabelId,
    LocalLabelRevisionId RevisionId,
    ProductDocumentId? ContentId,
    GlobalLabelVersionId? DerivedFromGlobalLabelVersionId,
    string? DataCarrierCode,
    string? ChangeSummary);
