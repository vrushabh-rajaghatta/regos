namespace RegOS.Labeling.Application.Queries.ListLocalLabelRevisions;

/// <param name="DerivedFromGlobalLabelVersionNumber">
/// Which core version this revision was written from, resolved to its number
/// for display. Null is legitimate and does not mean "unknown error" — see
/// EPIC-018 D3.
/// </param>
public sealed record LocalLabelRevisionSummary(
    Guid Id,
    int RevisionNumber,
    string Status,
    Guid? ContentId,
    Guid? DerivedFromGlobalLabelVersionId,
    int? DerivedFromGlobalLabelVersionNumber,
    string? DataCarrierCode,
    string? ChangeSummary,
    DateOnly? ApprovedOn,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo);
