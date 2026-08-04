using RegOS.Labeling.Domain.Aggregates.LocalLabels;

namespace RegOS.Labeling.Application.Commands.DiscardLocalLabelDraft;

/// <summary>
/// Throws away the revision being prepared. Only ever a draft — an approved
/// labelling document is a controlled record.
/// </summary>
public sealed record DiscardLocalLabelDraftCommand(LocalLabelId LocalLabelId);
