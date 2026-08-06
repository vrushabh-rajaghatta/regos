namespace RegOS.Process.Application.Queries.GetProcessPlan;

/// <summary>
/// One scheduled step. <b>Both dates are inclusive</b> — a five-day step
/// starting on the 1st ends on the 5th.
/// </summary>
/// <param name="Predecessors">
/// What this waits for, by code. The plan carries its own copy of the graph, so
/// reading it never touches the playbook it came from.
/// </param>
/// <param name="ActualStartOn">
/// <b>Derived from history, never stored.</b> Null after a step completed
/// without a recorded start — nobody recorded one, so RegOS does not know one.
/// </param>
public sealed record PlannedStepDetails(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    Guid? ParentStepId,
    int Order,
    DateOnly PlannedStartOn,
    DateOnly PlannedEndOn,
    IReadOnlyList<string> Predecessors,
    string Status,
    DateOnly? ActualStartOn,
    DateOnly? ActualEndOn,
    bool IsSettled,
    IReadOnlyList<StepHistoryEntry> History,
    IReadOnlyList<AttachedArtefact> Attached);

/// <summary>
/// A regulatory record somebody said contributes to this step.
/// </summary>
/// <remarks>
/// <b>Read-only, and Process holds no foreign key</b> (ADR-065 D2). The owning
/// aggregate carries the column; this composes the reverse view over
/// <c>RegOSDbContext</c>, which ADR-016 permits and the repository guard is what
/// keeps to a read.
/// <para>
/// <b>An empty list means nothing</b> (I9). A step with no attachments may have
/// produced nothing, or may simply not have been annotated — and the second is
/// the common case for work recorded before anyone planned it.
/// </para>
/// </remarks>
public sealed record AttachedArtefact(string Kind, Guid Id, string Title);

/// <param name="OccurredOn">The business date. <param name="RecordedOnUtc">When
/// RegOS learned. Two clocks, always (ADR-037), and append-only (I6).</param></param>
public sealed record StepHistoryEntry(
    string Status,
    DateOnly OccurredOn,
    DateTime RecordedOnUtc,
    string? Note);
