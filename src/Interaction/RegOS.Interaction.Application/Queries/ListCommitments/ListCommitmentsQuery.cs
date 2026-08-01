namespace RegOS.Interaction.Application.Queries.ListCommitments;

/// <param name="IncludeClosed">
/// Fulfilled and waived commitments are hidden by default: the list answers
/// "what do we still owe?" A regulatory record is never deleted, only filtered.
/// </param>
public sealed record ListCommitmentsQuery(bool IncludeClosed = false);
