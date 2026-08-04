namespace RegOS.Labeling.Application.Queries.ListIndications;

/// <param name="ConditionCode">
/// The join key. <em>Type 2 diabetes mellitus</em> and <em>Diabète sucré de
/// type 2</em> share it; the label texts do not.
/// </param>
public sealed record IndicationSummary(
    Guid Id,
    string ConditionCode,
    string ConditionDisplay,
    string ConditionSystem,
    string LabelText,
    string CurrentStatus,
    DateOnly CurrentStatusOccurredOn,
    IReadOnlyList<PopulationSummary> Populations,
    IReadOnlyList<OtherTherapySummary> OtherTherapies,
    IReadOnlyList<IndicationDecisionSummary> History);

/// <param name="AgeLow">
/// Null means "from birth"; a null <paramref name="AgeHigh"/> means "and
/// above". Neither is missing data.
/// </param>
public sealed record PopulationSummary(
    Guid Id,
    int? AgeLow,
    int? AgeHigh,
    string? AgeUnitCode,
    string? AgeUnitDisplay,
    string GenderCode,
    string GenderDisplay,
    string? PhysiologicalConditionCode,
    string? PhysiologicalConditionDisplay,
    string? Description);

public sealed record OtherTherapySummary(
    Guid Id,
    string RelationshipCode,
    string RelationshipDisplay,
    string Therapy);

/// <param name="RecordedOnUtc">
/// When RegOS learned of the decision, as against when it took effect. Kept
/// apart because both get asked about.
/// </param>
public sealed record IndicationDecisionSummary(
    Guid Id,
    string Status,
    DateOnly OccurredOn,
    DateTime RecordedOnUtc,
    string? Note);
