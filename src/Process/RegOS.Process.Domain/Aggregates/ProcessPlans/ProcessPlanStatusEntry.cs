using RegOS.SharedKernel.Abstractions;

namespace RegOS.Process.Domain.Aggregates.ProcessPlans;

/// <summary>One dated point in a plan's history.</summary>
/// <remarks>
/// The ninth append-only history, and the eighth user of the shared EF mapping
/// (ADR-046 decision 6). The configuration is shared; the type and its rules are
/// not — the scope ADR-042 set, and the measurement that set it still holds.
/// </remarks>
public sealed class ProcessPlanStatusEntry : Entity<ProcessPlanStatusEntryId>
{
    public const int NoteMaxLength = 500;

    // EF materialisation.
    private ProcessPlanStatusEntry()
    {
    }

    internal ProcessPlanStatusEntry(
        ProcessPlanStatusEntryId id,
        ProcessPlanStatus status,
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

    public ProcessPlanStatus Status { get; private set; }

    /// <summary>The business date — when this became true.</summary>
    public DateOnly OccurredOn { get; private set; }

    /// <summary>
    /// The system timestamp — when RegOS learned.
    /// </summary>
    /// <remarks>
    /// <b>The one wall clock I5 does not constrain</b>, and the exclusion is
    /// deliberate: this records when a row was written, not what the plan says.
    /// Two instantiations from identical inputs produce identical schedules and
    /// different <c>RecordedOnUtc</c> values, and both facts are correct.
    /// </remarks>
    public DateTime RecordedOnUtc { get; private set; }

    public string? Note { get; private set; }
}
