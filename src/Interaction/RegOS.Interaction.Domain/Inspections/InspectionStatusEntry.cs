namespace RegOS.Interaction.Domain.Inspections;

/// <summary>
/// One dated point in an inspection's history.
/// </summary>
/// <remarks>
/// The sixth append-only history. When it was written the extraction ledger had
/// closed at five as <b>refused</b> rather than deferred, so this was
/// deliberately another hand-written copy.
/// <para>
/// <b>EPIC-004 S003 reopened it and the refusal did not survive the
/// measurement.</b> Counting owned configurations rather than histories, four
/// of five were line-for-line identical — so the mapping moved to
/// <c>StatusHistoryMapping</c> (ADR-046 decision 6). What was refused then and
/// is still refused now is extracting <em>this type</em>: ADR-042's finding
/// that structural similarity is not behavioural similarity is unchanged, and
/// an <c>InspectionStatus</c> is not a <c>CommitmentStatus</c>.
/// </para>
/// </remarks>
public sealed class InspectionStatusEntry
{
    public const int NoteMaxLength = 500;

    internal InspectionStatusEntry(
        InspectionStatusEntryId id,
        InspectionStatus status,
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

    public InspectionStatusEntryId Id { get; }

    public InspectionStatus Status { get; }

    public DateOnly OccurredOn { get; }

    public DateTime RecordedOnUtc { get; }

    public string? Note { get; }
}

public readonly record struct InspectionStatusEntryId(Guid Value)
{
    public static InspectionStatusEntryId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
