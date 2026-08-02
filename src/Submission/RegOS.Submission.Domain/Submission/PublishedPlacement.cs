using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.Blueprint;

namespace RegOS.Submission.Domain.Submission;

/// <summary>
/// One placement in the previously published sequence — the baseline a filing is
/// measured against.
/// </summary>
/// <remarks>
/// Supplied to <see cref="Submission.Publish"/> by the application layer, the
/// same way the sequence number and the timestamp are: a Submission is a root,
/// so the sequence before it is outside its consistency boundary. <b>The facts
/// come from outside; the rule that reads them lives in the aggregate</b>
/// (ADR-044 decision 6, ADR-045).
/// <para>
/// <see cref="ProductDocumentId"/> and <see cref="TemplateSectionId"/> together
/// are the identity that survives across sequences — <em>the same document, in
/// the same place</em>. A <c>SubmissionDocumentId</c> belongs to one submission
/// and cannot serve; it is carried only so a replacement can point at the
/// placement it supersedes.
/// </para>
/// </remarks>
public sealed record PublishedPlacement(
    SubmissionDocumentId Id,
    ProductDocumentId ProductDocumentId,
    TemplateSectionId TemplateSectionId,
    DocumentVersionId DocumentVersionId);
