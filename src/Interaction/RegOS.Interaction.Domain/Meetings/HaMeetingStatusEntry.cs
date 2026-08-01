namespace RegOS.Interaction.Domain.Meetings;

/// <summary>
/// One dated point in a meeting's history.
/// </summary>
/// <remarks>
/// <b>The fifth append-only history — the last measurement before the
/// extraction decision.</b> See the epic's extraction ledger: S003 found the
/// entry type and the chronology rule cost almost nothing to duplicate, and the
/// EF configuration is where the maintenance actually lives.
/// </remarks>
public sealed class HaMeetingStatusEntry
{
    public const int NoteMaxLength = 500;

    internal HaMeetingStatusEntry(
        HaMeetingStatusEntryId id,
        HaMeetingStatus status,
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

    public HaMeetingStatusEntryId Id { get; }

    public HaMeetingStatus Status { get; }

    public DateOnly OccurredOn { get; }

    public DateTime RecordedOnUtc { get; }

    public string? Note { get; }
}

public readonly record struct HaMeetingStatusEntryId(Guid Value)
{
    public static HaMeetingStatusEntryId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
