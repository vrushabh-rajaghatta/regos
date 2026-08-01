namespace RegOS.Api.Endpoints.Inspections;

/// <param name="Findings">
/// What the authority found. What those findings oblige is a Commitment, with
/// its own due date and owner.
/// </param>
public sealed record RecordInspectionFindingsRequest(string? Findings);
