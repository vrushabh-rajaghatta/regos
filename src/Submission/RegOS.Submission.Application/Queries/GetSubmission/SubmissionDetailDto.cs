namespace RegOS.Submission.Application.Queries.GetSubmission;

public sealed record SubmissionDetailDto(
    Guid Id,
    string Title,
    Guid ApplicationId,
    string ApplicationName,
    Guid SubmissionTypeId,
    string SubmissionTypeName,
    string Status,
    // What this will be rendered as when it leaves RegOS. Editable while a
    // draft, frozen once published (ADR-047).
    string Format,
    DateTime CreatedOn,
    // The blueprint this submission is judged against, pinned at creation.
    // Null when no published template governs its submission type.
    BoundTemplateDto? BoundTemplate,
    // What this was filed as. Null while a draft (ADR-044 decision 4).
    int? SequenceNumber,
    // What it *would* be filed as if published now — MAX(published) + 1 in this
    // application. A projection, stored nowhere, and deliberately distinct from
    // SequenceNumber: one is a fact, the other an expectation. Sent for
    // published submissions too, and simply not shown.
    int NextSequenceNumber,
    // Its own lifecycle, oldest first. Only steps we are the actor of — what
    // the authority did arrives as correspondence anchored to this submission
    // (ADR-046), and the page composes the two rather than the backend joining
    // across bounded contexts.
    IReadOnlyList<SubmissionStatusStep> History);

/// <param name="OccurredOn">
/// When the step happened, as a regulator would date it — which for a migrated
/// filing is years before RegOS was told.
/// </param>
public sealed record SubmissionStatusStep(
    string Status,
    DateOnly OccurredOn,
    DateTime RecordedOnUtc,
    string? Note);

/// <summary>
/// The published template version a submission is bound to, carrying the names
/// the UI needs ("FDA IND (CTD) v1") so reading a submission never costs a
/// second call to reference data.
/// </summary>
public sealed record BoundTemplateDto(
    Guid TemplateVersionId,
    Guid TemplateId,
    string TemplateCode,
    string TemplateName,
    int VersionNumber);
