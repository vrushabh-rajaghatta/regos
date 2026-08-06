namespace RegOS.Interaction.Application.Queries.GetCorrespondence;

/// <summary>
/// Everything the correspondence page shows.
/// </summary>
/// <remarks>
/// It returns no questions and no attachments — those arrive with the stories
/// that build them (S003, S002). What it does carry is <see cref="RecordedOnUtc"/>
/// beside <see cref="OccurredOn"/>: a letter logged today may be dated 2019, and
/// a reader who cannot see both will eventually mistake one for the other.
/// </remarks>
public sealed record CorrespondenceDetail(
    Guid CorrespondenceId,
    string Direction,
    string Subject,
    DateOnly OccurredOn,
    DateOnly? ResponseDueOn,
    string? AuthorityReference,
    DateTime RecordedOnUtc,
    Guid AuthorityId,
    string AuthorityName,
    Guid CorrespondenceTypeId,
    string CorrespondenceTypeName,
    Guid? AuthorityDivisionId,
    string? AuthorityDivisionName,
    Guid? RegulatoryApplicationId,
    string? RegulatoryApplicationName,
    string? RegulatoryApplicationNumber,
    Guid? SubmissionId,
    Guid? RegistrationId,
    /// <summary>
    /// The planned work this letter serves, if any (ADR-065 D2). Null is the
    /// ordinary state and means nothing — most correspondence was recorded
    /// before anyone planned anything (I9).
    /// </summary>
    Guid? ProcessStepId,
    IReadOnlyList<CorrespondenceAttachmentSummary> Attachments,
    IReadOnlyList<CorrespondenceQuestionSummary> Questions);

/// <summary>One question, with the history it has accumulated.</summary>
public sealed record CorrespondenceQuestionSummary(
    Guid QuestionId,
    string Number,
    string Text,
    DateOnly? TargetResponseOn,
    string? ResponseText,
    string CurrentStatus,
    DateOnly? RespondedOn,
    IReadOnlyList<QuestionHistoryEntry> History);

/// <summary>One dated point in a question's history.</summary>
public sealed record QuestionHistoryEntry(
    string Status,
    DateOnly OccurredOn,
    DateTime RecordedOnUtc,
    string? Note);

/// <summary>One attached file, as the page lists it.</summary>
public sealed record CorrespondenceAttachmentSummary(
    Guid AttachmentId,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    DateTime UploadedOnUtc);
