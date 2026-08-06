using RegOS.SharedKernel.Abstractions;

namespace RegOS.Process.Domain.Aggregates.ProcessObjectives;

/// <summary>
/// One dated point in an objective's history.
/// </summary>
/// <remarks>
/// <b>The seventh append-only history, and still not extracted.</b> EPIC-006
/// measured this at three occurrences and again at six: the entry type is ~30
/// lines, the chronology rule is one line, and the real duplication was the EF
/// configuration — which <em>is</em> shared (<c>StatusHistoryMapping</c>,
/// ADR-046 decision 6). The measurement said leave the type alone, and it still
/// says so.
/// <para>
/// Unlike its six predecessors this one inherits <c>Entity</c> and its id is a
/// class, not a <c>record struct</c> (ES-020 / ADR-043). The older ones are the
/// pending migration CLAUDE.md names; copying one would have propagated it.
/// </para>
/// </remarks>
public sealed class ProcessObjectiveStatusEntry : Entity<ProcessObjectiveStatusEntryId>
{
    public const int NoteMaxLength = 500;

    // EF materialisation.
    private ProcessObjectiveStatusEntry()
    {
    }

    internal ProcessObjectiveStatusEntry(
        ProcessObjectiveStatusEntryId id,
        ProcessObjectiveStatus status,
        DateOnly occurredOn,
        DateTime recordedOnUtc,
        string? note)
    {
        Id = id;
        Status = status;
        OccurredOn = occurredOn;
        RecordedOnUtc = recordedOnUtc;
        Note = note;
    }

    public ProcessObjectiveStatus Status { get; private set; }

    /// <summary>The business date — when this became true.</summary>
    public DateOnly OccurredOn { get; private set; }

    /// <summary>The system timestamp — when RegOS learned.</summary>
    public DateTime RecordedOnUtc { get; private set; }

    public string? Note { get; private set; }
}
