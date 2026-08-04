using RegOS.Labeling.Application.Queries.ListIndications;

namespace RegOS.Labeling.Application.Queries.ListUndesirableEffects;

/// <param name="EffectCode">
/// The join key — the same effect is recognisable across markets through it,
/// while the label text is one market's wording.
/// </param>
/// <param name="FrequencyCode">
/// Null is ordinary: a label may list an effect without stating a band, and
/// <em>not known</em> is itself one.
/// </param>
public sealed record UndesirableEffectSummary(
    Guid Id,
    string EffectCode,
    string EffectDisplay,
    string EffectSystem,
    string LabelText,
    string? FrequencyCode,
    string? FrequencyDisplay,
    IReadOnlyList<PopulationSummary> Populations);
