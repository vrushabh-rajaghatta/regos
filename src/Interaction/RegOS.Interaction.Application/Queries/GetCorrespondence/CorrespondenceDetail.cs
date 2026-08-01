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
    Guid? RegulatoryApplicationId,
    string? RegulatoryApplicationName,
    string? RegulatoryApplicationNumber,
    Guid? SubmissionId,
    Guid? RegistrationId);
