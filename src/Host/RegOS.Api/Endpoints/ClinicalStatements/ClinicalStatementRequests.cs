namespace RegOS.Api.Endpoints.ClinicalStatements;

/// <summary>
/// The request shapes contraindications and undesirable effects share.
/// </summary>
/// <remarks>
/// One wire shape rather than two, because the population fields are one
/// type mapped three times — a second identical record would be duplication
/// with no reader.
/// </remarks>
public sealed record StatementPopulationRequest(
    int? AgeLow,
    int? AgeHigh,
    string? AgeUnitCode,
    string GenderCode,
    string? PhysiologicalConditionCode,
    string? Description);

public sealed record RestateStatementTextRequest(string LabelText);

public sealed record ClinicalStatementResponse(Guid Id);
