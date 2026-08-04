using RegOS.Labeling.Domain.Aggregates.LocalLabels;

namespace RegOS.Labeling.Application.Commands.CreateLocalLabel;

public sealed record CreateLocalLabelResult(
    LocalLabelId Id,
    LocalLabelRevisionId DraftRevisionId);
