using RegOS.Study.Domain.Aggregates.ClinicalStudy;
using RegOS.Study.Domain.Aggregates.NonClinicalStudy;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Commands.ReportStudyOnPlacement;

/// <summary>
/// States which study a placement reports — or, with both ids null, that it
/// reports none.
/// </summary>
/// <remarks>
/// <b>Two typed properties rather than a <c>(kind, id)</c> pair.</b> A clinical
/// and a non-clinical study are different aggregates with different identity
/// types (ADR-056), and a discriminator here is where a discriminator in the
/// domain would come from. The exclusive-or is checked by the handler and
/// enforced structurally by the aggregate, whose two writers each clear the
/// other.
/// <para>
/// It expresses the whole fact rather than a delta, so sending it twice lands in
/// the same place — the shape <c>PlaceSubmissionDocumentCommand</c> uses for the
/// same reason.
/// </para>
/// </remarks>
/// <param name="FileTag">
/// What role the document plays in that study's report — one of ICH's 97
/// published tokens. Null is legitimate: a placement can name its study before
/// anyone has decided what it contributes.
/// </param>
public sealed record ReportStudyOnPlacementCommand(
    SubmissionId SubmissionId,
    SubmissionDocumentId SubmissionDocumentId,
    ClinicalStudyId? ClinicalStudyId,
    NonClinicalStudyId? NonClinicalStudyId,
    string? FileTag = null);
