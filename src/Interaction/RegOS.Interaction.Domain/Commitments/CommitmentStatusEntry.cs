namespace RegOS.Interaction.Domain.Commitments;

/// <summary>
/// One dated point in a commitment's history.
/// </summary>
/// <remarks>
/// <b>The fourth append-only history, and still not extracted — the type,
/// that is.</b> S003 measured the three that existed: the entry type is ~30
/// lines of code, the chronology rule is one line, and the real duplication is
/// the EF configuration. That measurement held. The configuration is now shared
/// (<c>StatusHistoryMapping</c>, ADR-046 decision 6); this type and its rules
/// stayed exactly where they were, which is what the measurement asked for.
/// </remarks>
public sealed class CommitmentStatusEntry
{
    public const int NoteMaxLength = 500;

    internal CommitmentStatusEntry(
        CommitmentStatusEntryId id,
        CommitmentStatus status,
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

    public CommitmentStatusEntryId Id { get; }

    public CommitmentStatus Status { get; }

    /// <summary>The business date — when this became true.</summary>
    public DateOnly OccurredOn { get; }

    /// <summary>The system timestamp — when RegOS learned.</summary>
    public DateTime RecordedOnUtc { get; }

    public string? Note { get; }
}

public readonly record struct CommitmentStatusEntryId(Guid Value)
{
    public static CommitmentStatusEntryId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
