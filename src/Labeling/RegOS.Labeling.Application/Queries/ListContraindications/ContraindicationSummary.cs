using RegOS.Labeling.Application.Queries.ListIndications;

namespace RegOS.Labeling.Application.Queries.ListContraindications;

/// <param name="ConditionCode">
/// The join key — the same statement is recognisable across markets through it,
/// while the label text is one market's wording.
/// </param>
public sealed record ContraindicationSummary(
    Guid Id,
    string ConditionCode,
    string ConditionDisplay,
    string ConditionSystem,
    string LabelText,
    IReadOnlyList<PopulationSummary> Populations);
