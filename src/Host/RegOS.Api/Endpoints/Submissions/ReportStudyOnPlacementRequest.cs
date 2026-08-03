namespace RegOS.Api.Endpoints.Submissions;

/// <param name="ClinicalStudyId">
/// The clinical study this document reports. Null when it reports a
/// non-clinical one, or none.
/// </param>
/// <param name="NonClinicalStudyId">
/// The non-clinical study this document reports.
/// </param>
/// <remarks>
/// Two typed fields rather than a <c>(kind, id)</c> pair: they name two
/// aggregates (ADR-056), and a kind discriminator on the wire is where one in
/// the domain would start. Both null clears the reported study — the body
/// states the whole fact, so sending it twice lands in the same place.
/// </remarks>
public sealed record ReportStudyOnPlacementRequest(
    Guid? ClinicalStudyId,
    Guid? NonClinicalStudyId);
