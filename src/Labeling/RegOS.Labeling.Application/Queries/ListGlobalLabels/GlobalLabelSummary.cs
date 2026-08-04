namespace RegOS.Labeling.Application.Queries.ListGlobalLabels;

/// <summary>
/// A label as a list row: what it is, what is in force, and whether someone is
/// currently writing the next issue.
/// </summary>
/// <param name="VersionInForceNumber">
/// Null while the first draft is still being written — an ordinary state, and
/// the screen says so rather than showing a blank.
/// </param>
/// <param name="DraftVersionId">
/// The open draft, if any. At most one, which is why this is an id and not a
/// list.
/// </param>
public sealed record GlobalLabelSummary(
    Guid Id,
    string Name,
    string LabelTypeCode,
    string LabelTypeDisplay,
    string LabelTypeSystem,
    int? VersionInForceNumber,
    DateOnly? EffectiveFrom,
    Guid? DraftVersionId,
    int? DraftVersionNumber,
    int VersionCount);
