using RegOS.SharedKernel.Abstractions;

namespace RegOS.Process.Domain.Aggregates.ProcessPlans;

/// <summary>One dated point in a step's execution history.</summary>
/// <remarks>
/// <b>Append-only</b> (ADR-065 I6). A correction is a new entry, never a rewrite:
/// <em>"we thought this was done on the 3rd"</em> is itself regulatory history,
/// and the two clocks ADR-037 established are what make that readable.
/// <para>
/// The tenth append-only history in RegOS, and the ninth user of the shared EF
/// mapping. The measurement that refused to extract the <em>type</em> still
/// holds: the entry is thirty lines and its rules are this aggregate's alone.
/// </para>
/// </remarks>
public sealed class ProcessStepStatusEntry : Entity<ProcessStepStatusEntryId>
{
    public const int NoteMaxLength = 500;

    // EF materialisation.
    private ProcessStepStatusEntry()
    {
    }

    internal ProcessStepStatusEntry(
        ProcessStepStatusEntryId id,
        ProcessStepStatus status,
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

    public ProcessStepStatus Status { get; private set; }

    /// <summary>The business date — when this became true.</summary>
    public DateOnly OccurredOn { get; private set; }

    /// <summary>The system timestamp — when RegOS learned.</summary>
    public DateTime RecordedOnUtc { get; private set; }

    /// <summary>
    /// Why. <b>Required when the status is <see cref="ProcessStepStatus.Skipped"/></b>
    /// — a skipped step with no reason is an unexplained gap in a regulatory
    /// record a year later, and <em>"Skipped"</em> on its own is not an
    /// explanation.
    /// </summary>
    public string? Note { get; private set; }
}
