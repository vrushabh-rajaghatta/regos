namespace RegOS.Labeling.Application.Queries.GetLabelLanguageCoverage;

/// <param name="Expected">
/// What this market's labelling is normally in, from the country.
/// </param>
/// <param name="Recorded">
/// The languages local labels actually exist in. May legitimately include
/// languages that are not expected — an extra translation is nobody's error.
/// </param>
/// <param name="Missing">
/// Expected but not recorded. <b>An observation, never a refusal</b> — the
/// caller is a screen that says so, not a gate.
/// </param>
public sealed record LabelLanguageCoverage(
    IReadOnlyList<string> Expected,
    IReadOnlyList<string> Recorded,
    IReadOnlyList<string> Missing);
