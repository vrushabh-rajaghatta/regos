using RegOS.SharedKernel.Abstractions;

namespace RegOS.Labeling.Domain.Aggregates.Indications;

/// <summary>
/// One regulatory decision about this indication, and the day it was taken.
/// </summary>
/// <remarks>
/// <b>Append-only, and not for audit's sake.</b> A regulatory decision should
/// not disappear: an indication must not silently become withdrawn, it must
/// have <em>become withdrawn on a date</em>. That is a different thing from
/// revisioning a document, and it is why this exists instead of
/// <c>IndicationRevision</c>.
/// </remarks>
public sealed class IndicationStatusEntry : Entity<IndicationStatusEntryId>
{
    public const int NoteMaxLength = 1000;

    internal IndicationStatusEntry(
        IndicationStatusEntryId id,
        IndicationStatus status,
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

    public IndicationStatus Status { get; private set; }

    /// <summary>The business date the authority's decision took effect.</summary>
    public DateOnly OccurredOn { get; private set; }

    /// <summary>When RegOS learned of it — a different date, and both are asked about.</summary>
    public DateTime RecordedOnUtc { get; private set; }

    public string? Note { get; private set; }
}
