namespace RegOS.Interaction.Domain.Inspections;

/// <summary>
/// One dated point in an inspection's history.
/// </summary>
/// <remarks>
/// The sixth append-only history. The extraction ledger closed at five as
/// <b>refused</b> rather than deferred — the five configurations turned out not
/// to be five copies of one shape — so this is deliberately another hand-written
/// copy rather than the consumer that finally justifies a helper.
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
