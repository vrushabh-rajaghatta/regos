namespace RegOS.Interaction.Application.Queries.ListCommitments;

public sealed record CommitmentSummary(
    Guid CommitmentId,
    string Title,
    string? Description,
    Guid AuthorityId,
    string AuthorityName,
    DateOnly GivenOn,
    DateOnly DueOn,
    DateOnly? FulfilledOn,
    Guid? OwnerUserId,
    Guid? SourceCorrespondenceId,
    string CurrentStatus,
    IReadOnlyList<CommitmentHistoryEntry> History);

public sealed record CommitmentHistoryEntry(
    string Status,
    DateOnly OccurredOn,
    DateTime RecordedOnUtc,
    string? Note);
