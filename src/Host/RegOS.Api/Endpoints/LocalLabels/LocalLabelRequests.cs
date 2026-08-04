namespace RegOS.Api.Endpoints.LocalLabels;

/// <param name="LabelTypeCode">
/// A code from <c>/api/labels/vocabulary</c>'s <c>localLabelTypes</c> — carton
/// artwork is one of them.
/// </param>
public sealed record CreateLocalLabelRequest(
    string LabelTypeCode,
    string Language);

/// <param name="DerivedFromGlobalLabelVersionId">
/// Null is legitimate: a migrated portfolio does not know which core version a
/// historical revision came from (EPIC-018 D3).
/// </param>
public sealed record PrepareLocalLabelRevisionRequest(
    Guid? ContentId,
    Guid? DerivedFromGlobalLabelVersionId,
    string? DataCarrierCode,
    string? ChangeSummary);

/// <param name="ApprovedOn">
/// When the authority approved it — a different fact from when it takes effect,
/// and both are recorded.
/// </param>
public sealed record PublishLocalLabelRevisionRequest(
    DateOnly ApprovedOn,
    DateOnly EffectiveFrom);

public sealed record LocalLabelResponse(Guid Id, Guid DraftRevisionId);

public sealed record LocalLabelRevisionResponse(Guid Id, int RevisionNumber);
