using RegOS.Labeling.Domain.Aggregates.GlobalLabels;

namespace RegOS.Labeling.Application.Queries.ListGlobalLabelVersions;

/// <summary>Every issue this label has had, and when each was in force.</summary>
public sealed record ListGlobalLabelVersionsQuery(GlobalLabelId GlobalLabelId);
