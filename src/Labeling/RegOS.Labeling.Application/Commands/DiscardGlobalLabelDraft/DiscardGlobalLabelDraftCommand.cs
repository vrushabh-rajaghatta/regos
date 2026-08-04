using RegOS.Labeling.Domain.Aggregates.GlobalLabels;

namespace RegOS.Labeling.Application.Commands.DiscardGlobalLabelDraft;

/// <summary>
/// Throws away the open draft. Only ever a draft — see
/// <c>GlobalLabel.DiscardDraft</c> for why that is not a hole in ES-018.
/// </summary>
public sealed record DiscardGlobalLabelDraftCommand(GlobalLabelId GlobalLabelId);
