namespace RegOS.Labeling.Application.Queries.ListGlobalLabelVersions;

/// <param name="EffectiveTo">
/// The last day this issue was in force. Null on the current one and on a
/// draft — the difference being that the current one has an
/// <c>EffectiveFrom</c>.
/// </param>
/// <param name="PublishedOnUtc">
/// When the publish happened, as against when it took effect. Kept apart
/// because a version approved in March to apply from June has two dates and
/// somebody asks about each.
/// </param>
public sealed record GlobalLabelVersionSummary(
    Guid Id,
    int VersionNumber,
    string Status,
    Guid? ContentId,
    string? ChangeSummary,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo,
    DateTime? PublishedOnUtc);
