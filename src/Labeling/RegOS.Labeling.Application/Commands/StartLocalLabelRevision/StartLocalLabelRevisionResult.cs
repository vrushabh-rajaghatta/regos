using RegOS.Labeling.Domain.Aggregates.LocalLabels;

namespace RegOS.Labeling.Application.Commands.StartLocalLabelRevision;

public sealed record StartLocalLabelRevisionResult(
    LocalLabelRevisionId Id,
    int RevisionNumber);
