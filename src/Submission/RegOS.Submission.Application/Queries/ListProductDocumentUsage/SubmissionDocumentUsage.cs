namespace RegOS.Submission.Application.Queries.ListProductDocumentUsage;

/// <summary>
/// One event in a document's filing history — a cross-context read model
/// composed from SubmissionDocument, SubmissionDeletion, Submission,
/// Application, Authority, and the pinned DocumentVersion. Not a domain entity;
/// the Product Document aggregate is never loaded to build it.
///
/// Deliberately a flat record so it can grow later (submission status,
/// published-on, sequence number, validation status) without reshaping the
/// query's consumers.
/// </summary>
/// <remarks>
/// <para>
/// <b>The read reunifies what the write deliberately split.</b> A placement can
/// be frozen at publish; an absence cannot, so S002 wrote withdrawals down as
/// <c>SubmissionDeletion</c> rows rather than as <c>SubmissionDocument</c>s
/// carrying a flag (ADR-045). Read backwards the two are one chronological
/// stream, and <see cref="Operation"/> is what tells them apart.
/// </para>
/// <para>
/// The merge works because both sides carry the diff key —
/// <c>(ProductDocumentId, TemplateSectionId)</c>, <em>the same document in the
/// same place</em>. Nothing is reconstructed and nothing is matched by
/// guesswork, which is what EPIC-004 S006 set out to find out.
/// </para>
/// <para>
/// Grown rather than replaced: sequence number and status were two of the
/// candidates this record's original note named.
/// </para>
/// </remarks>
/// <param name="SequenceNumber">
/// What the filing was numbered. Null while a draft (ADR-044).
/// </param>
/// <param name="Operation">
/// What this filing did with the document — <c>New</c>, <c>Replace</c>,
/// <c>Unchanged</c> or <c>Delete</c>. Null while a draft: the operation is
/// derived and frozen at publish, so a draft has not yet done anything.
/// </param>
/// <param name="VersionNumber">
/// The version this filing pinned. <b>Null exactly when the event is a
/// withdrawal</b> — nothing was placed, which is the point of it.
/// </param>
/// <param name="AttachedOnUtc">Null for a withdrawal, for the same reason.</param>
public sealed record SubmissionDocumentUsage(
    Guid SubmissionId,
    Guid ApplicationId,
    string ApplicationName,
    string SubmissionTitle,
    string ApplicationType,
    string Authority,
    int? SequenceNumber,
    string Status,
    string Format,
    string? Operation,
    int? VersionNumber,
    DateTime? AttachedOnUtc);
