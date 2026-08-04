using RegOS.Labeling.Domain.Aggregates.GlobalLabels;

namespace RegOS.Labeling.Application.Commands.StartGlobalLabelDraft;

public sealed record StartGlobalLabelDraftResult(
    GlobalLabelVersionId Id,
    int VersionNumber);
