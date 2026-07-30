namespace RegOS.Submission.Application.Queries.GetSubmission;

public sealed record SubmissionDetailDto(
    Guid Id,
    string Title,
    Guid ApplicationId,
    string ApplicationName,
    Guid SubmissionTypeId,
    string SubmissionTypeName,
    string Status,
    DateTime CreatedOn,
    // The blueprint this submission is judged against, pinned at creation.
    // Null when no published template governs its submission type.
    BoundTemplateDto? BoundTemplate);

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
