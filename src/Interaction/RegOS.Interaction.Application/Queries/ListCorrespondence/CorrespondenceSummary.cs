namespace RegOS.Interaction.Application.Queries.ListCorrespondence;

/// <summary>
/// One row of the correspondence list.
/// </summary>
/// <remarks>
/// <see cref="ResponseDueOn"/> is returned as the date it is, not as
/// "due in 9 days". Proximity is the reader's interpretation of a fact, and it
/// changes every midnight — deriving it at the edge keeps one clock in play
/// rather than three (ADR-037).
/// </remarks>
public sealed record CorrespondenceSummary(
    Guid CorrespondenceId,
    string Direction,
    string Subject,
    DateOnly OccurredOn,
    DateOnly? ResponseDueOn,
    string? AuthorityReference,
    Guid AuthorityId,
    string AuthorityName,
    Guid CorrespondenceTypeId,
    string CorrespondenceTypeName,
    Guid? RegulatoryApplicationId,
    string? RegulatoryApplicationNumber);
