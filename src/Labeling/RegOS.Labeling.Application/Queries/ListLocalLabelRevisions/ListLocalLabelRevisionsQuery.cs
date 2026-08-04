using RegOS.Labeling.Domain.Aggregates.LocalLabels;

namespace RegOS.Labeling.Application.Queries.ListLocalLabelRevisions;

/// <summary>This market's regulatory history for one labelling document.</summary>
public sealed record ListLocalLabelRevisionsQuery(LocalLabelId LocalLabelId);
