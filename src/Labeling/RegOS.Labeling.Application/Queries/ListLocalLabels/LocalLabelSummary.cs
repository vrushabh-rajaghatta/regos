namespace RegOS.Labeling.Application.Queries.ListLocalLabels;

/// <param name="RevisionInForceNumber">
/// Null before the first approval — an ordinary state, and the screen says so
/// rather than showing a blank.
/// </param>
/// <param name="PackagedProductId">
/// The pack this document is printed for, when it is printed for one — a carton
/// for the 30 is not the carton for the 100, even when the words are identical.
/// Null on most labels and on every one nobody has linked yet; carried on all of
/// them because no rule may branch on the label type (EPIC-018 D2).
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
    int RevisionCount,
    Guid? PackagedProductId,
    string? PackDescription);
