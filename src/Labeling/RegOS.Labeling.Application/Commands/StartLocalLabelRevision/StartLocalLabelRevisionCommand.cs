using RegOS.Labeling.Domain.Aggregates.LocalLabels;

namespace RegOS.Labeling.Application.Commands.StartLocalLabelRevision;

/// <summary>Opens the next revision of this market's label for preparation.</summary>
public sealed record StartLocalLabelRevisionCommand(LocalLabelId LocalLabelId);
