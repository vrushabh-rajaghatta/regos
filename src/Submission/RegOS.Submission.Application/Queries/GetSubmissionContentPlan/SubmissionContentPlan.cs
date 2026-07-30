namespace RegOS.Submission.Application.Queries.GetSubmissionContentPlan;

/// <summary>
/// The dossier as a working surface: the bound blueprint's section tree, the
/// placeholders each section expects, and what currently fills them.
/// </summary>
/// <remarks>
/// Shaped around <em>placeholders</em>, not around today's storage. A
/// placeholder is a <c>RequiredDocument</c> right now, but the contract names
/// the concept — so when submission-owned placeholder state eventually earns its
/// own rows (an N/A justification, a reviewer comment, cardinality progress),
/// the implementation behind this changes and clients do not.
/// </remarks>
/// <param name="BoundTemplateVersionId">
/// Null when no published blueprint governs this submission. The envelope is
/// still returned, with every attached document listed as unplaced: "there is
/// no blueprint" is a state to render, not a failure to report.
/// </param>
public sealed record SubmissionContentPlan(
    Guid SubmissionId,
    Guid? BoundTemplateVersionId,
    string? TemplateName,
    int? VersionNumber,
    IReadOnlyList<ContentPlanSection> Sections,
    IReadOnlyList<ContentPlanDocument> UnplacedDocuments);

/// <param name="AdditionalDocuments">
/// Placed here, but satisfying no placeholder — a certificate's supporting
/// chromatograms, a statistical appendix. Legitimate dossier content, listed
/// rather than flagged: the hierarchy is organisational, and only placeholders
/// are a validation construct.
/// </param>
public sealed record ContentPlanSection(
    Guid SectionId,
    string Code,
    string Title,
    int Order,
    IReadOnlyList<ContentPlanPlaceholder> Placeholders,
    IReadOnlyList<ContentPlanDocument> AdditionalDocuments,
    IReadOnlyList<ContentPlanSection> Children);

/// <param name="PlaceholderId">
/// Stable: it is the required document's id, and the version it belongs to is
/// immutable. Clients may hold on to it.
/// </param>
public sealed record ContentPlanPlaceholder(
    Guid PlaceholderId,
    Guid DocumentTypeId,
    string DocumentTypeName,
    bool IsMandatory,
    int Order,
    bool IsSatisfied,
    IReadOnlyList<ContentPlanDocument> Documents);

public sealed record ContentPlanDocument(
    Guid SubmissionDocumentId,
    Guid ProductDocumentId,
    string Name,
    Guid DocumentTypeId,
    string DocumentTypeName,
    int VersionNumber,
    string FileName);
