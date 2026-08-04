namespace RegOS.Api.Endpoints.GlobalLabels;

/// <param name="LabelTypeCode">
/// A code from <c>/api/labels/vocabulary</c>, not a display name — the wire
/// carries the code so a re-worded term does not break a caller.
/// </param>
public sealed record CreateGlobalLabelRequest(
    string Name,
    string LabelTypeCode);

public sealed record AttachGlobalLabelContentRequest(Guid ContentId);

public sealed record PublishGlobalLabelVersionRequest(
    DateOnly EffectiveFrom,
    string? ChangeSummary);

public sealed record GlobalLabelResponse(Guid Id, Guid DraftVersionId);

public sealed record GlobalLabelVersionResponse(Guid Id, int VersionNumber);
