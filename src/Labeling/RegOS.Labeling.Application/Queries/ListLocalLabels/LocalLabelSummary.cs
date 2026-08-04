namespace RegOS.Labeling.Application.Queries.ListLocalLabels;

/// <param name="RevisionInForceNumber">
/// Null before the first approval — an ordinary state, and the screen says so
/// rather than showing a blank.
/// </param>
/// <param name="ApprovedOn">
/// Kept beside <paramref name="EffectiveFrom"/> rather than collapsed into it:
/// a market approved in May and effective in June has two answers, and a user
/// asks about each.
/// </param>
public sealed record LocalLabelSummary(
    Guid Id,
    string LabelTypeCode,
    string LabelTypeDisplay,
    string LabelTypeSystem,
    string Language,
    int? RevisionInForceNumber,
    DateOnly? ApprovedOn,
    DateOnly? EffectiveFrom,
    Guid? DraftRevisionId,
    int? DraftRevisionNumber,
    int RevisionCount);
