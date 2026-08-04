using RegOS.Labeling.Domain.Aggregates.GlobalLabels;

namespace RegOS.Labeling.Application.Commands.StartGlobalLabelDraft;

/// <summary>Opens the next issue of a label for writing.</summary>
public sealed record StartGlobalLabelDraftCommand(GlobalLabelId GlobalLabelId);
