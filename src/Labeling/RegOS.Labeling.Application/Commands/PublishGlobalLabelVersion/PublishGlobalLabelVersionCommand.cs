using RegOS.Labeling.Domain.Aggregates.GlobalLabels;

namespace RegOS.Labeling.Application.Commands.PublishGlobalLabelVersion;

/// <param name="EffectiveFrom">
/// The business date this issue takes effect — supplied, never read from the
/// clock. A version approved in March to apply from June is the ordinary case.
/// </param>
/// <param name="ChangeSummary">
/// What changed, in the author's words. Recorded on the draft and frozen with
/// it, so the sentence belongs to the version it describes.
/// </param>
public sealed record PublishGlobalLabelVersionCommand(
    GlobalLabelId GlobalLabelId,
    GlobalLabelVersionId VersionId,
    DateOnly EffectiveFrom,
    string? ChangeSummary);
