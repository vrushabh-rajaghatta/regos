using RegOS.Labeling.Domain.Aggregates.Indications;

namespace RegOS.Api.Endpoints.Indications;

/// <param name="ConditionCode">
/// From <c>/api/indications/vocabulary</c>. Coded so the same authorisation is
/// recognisable in every market; the text is what this market's label says.
/// </param>
public sealed record RecordIndicationRequest(
    string ConditionCode,
    string LabelText,
    DateOnly ApprovedOn);

public sealed record RestateIndicationTextRequest(string LabelText);

public sealed record RecordDecisionRequest(
    IndicationStatus Status,
    DateOnly OccurredOn,
    string? Note);

/// <param name="AgeLow">
/// Null means from birth; a null <paramref name="AgeHigh"/> means and above.
/// A bound without a unit is refused — 2 to 12 could be months or years.
/// </param>
public sealed record PopulationRequest(
    int? AgeLow,
    int? AgeHigh,
    string? AgeUnitCode,
    string GenderCode,
    string? PhysiologicalConditionCode,
    string? Description);

public sealed record TherapyRequest(string RelationshipCode, string Therapy);

public sealed record IndicationResponse(Guid Id);
