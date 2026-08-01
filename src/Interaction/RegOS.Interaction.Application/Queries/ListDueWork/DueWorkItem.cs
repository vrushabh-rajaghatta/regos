namespace RegOS.Interaction.Application.Queries.ListDueWork;

/// <summary>
/// One thing a person still has to do.
/// </summary>
/// <remarks>
/// <b>Everything interpretive is absent on purpose.</b> No <c>IsOverdue</c>, no
/// <c>DaysRemaining</c>, no "due this week" flag — those are readings of
/// <see cref="DueOn"/> against a clock, and they change every midnight. The
/// server returns the date; the edge decides what it means (ADR-037, and the
/// same call <c>ResponseDue</c> made in S001).
/// </remarks>
public sealed record DueWorkItem(
    string Kind,
    Guid Id,
    Guid? CorrespondenceId,
    string Title,
    string AuthorityName,
    DateOnly? DueOn,
    Guid? OwnerUserId,
    string Status);
