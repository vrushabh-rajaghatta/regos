namespace RegOS.Api.Endpoints.Correspondence;

/// <param name="Direction">
/// <c>Inbound</c> or <c>Outbound</c>. Stated rather than inferred from who sent
/// it — every real query begins with it (ADR-040 decision 6).
/// </param>
/// <param name="OccurredOn">
/// The date printed on the letter, not today. A letter logged now may be from
/// 2019, and both dates are kept.
/// </param>
/// <param name="ResponseDueOn">
/// Optional. Who owes the response follows from <paramref name="Direction"/> and
/// is never stored.
/// </param>
public sealed record RecordCorrespondenceRequest(
    Guid AuthorityId,
    Guid CorrespondenceTypeId,
    string Direction,
    string Subject,
    DateOnly OccurredOn,
    Guid? AuthorityDivisionId = null,
    DateOnly? ResponseDueOn = null,
    string? AuthorityReference = null,
    Guid? RegulatoryApplicationId = null,
    Guid? SubmissionId = null,
    Guid? RegistrationId = null);
