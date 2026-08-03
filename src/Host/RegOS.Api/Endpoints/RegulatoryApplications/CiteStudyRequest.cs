namespace RegOS.Api.Endpoints.RegulatoryApplications;

/// <remarks>
/// Two typed fields, not a <c>(kind, id)</c> pair — the same shape every study
/// reference on the wire takes (ADR-056 §2).
/// </remarks>
public sealed record CiteStudyRequest(
    Guid? ClinicalStudyId,
    Guid? NonClinicalStudyId);
