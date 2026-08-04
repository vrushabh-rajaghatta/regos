using RegOS.Labeling.Domain.Aggregates.GlobalLabels;

namespace RegOS.Labeling.Application.Commands.CreateGlobalLabel;

/// <param name="DraftVersionId">
/// The first draft, opened with the label. Returned so the caller can attach
/// content to it without a second round trip to find out what it is.
/// </param>
public sealed record CreateGlobalLabelResult(
    GlobalLabelId Id,
    GlobalLabelVersionId DraftVersionId);
