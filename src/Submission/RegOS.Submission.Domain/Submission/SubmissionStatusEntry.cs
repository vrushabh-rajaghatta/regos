using RegOS.SharedKernel.Abstractions;

namespace RegOS.Submission.Domain.Submission;

/// <summary>
/// One step in a submission's own lifecycle, recorded once and never changed.
/// </summary>
/// <remarks>
/// Two clocks, as every append-only history in RegOS has (ADR-037):
/// <list type="bullet">
/// <item><see cref="OccurredOn"/> — when it happened <em>in the world</em>. A
/// sequence filed in 2019 and migrated into RegOS today reads 2019.</item>
/// <item><see cref="RecordedOnUtc"/> — when <em>RegOS learned</em> of it.</item>
/// </list>
/// <para>
/// <b>This is the seventh such history, and the first written as a sealed-class
/// identity</b> (ES-020). The other six predate that rule and are on the
/// pending-migration list; copying one of them would have grown the list rather
/// than left it alone.
/// </para>
/// <para>
/// It also replaces a stored field: <c>Submission.PublishedAt</c> was exactly
/// the <see cref="RecordedOnUtc"/> of the <c>Published</c> entry, so it is
/// derived now rather than kept beside a history that could disagree with it —
/// the same call <c>Commitment.GivenOn</c> made.
/// </para>
/// </remarks>
public sealed class SubmissionStatusEntry : Entity<SubmissionStatusEntryId>
{
    public const int NoteMaxLength = 500;

    // EF materialisation only.
    private SubmissionStatusEntry()
    {
    }

    internal SubmissionStatusEntry(
        SubmissionStatusEntryId id,
        SubmissionStatus status,
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

    public SubmissionStatus Status { get; private set; }

    /// <summary>When the step happened, as a regulator would date it.</summary>
    public DateOnly OccurredOn { get; private set; }

    /// <summary>When RegOS was told.</summary>
    public DateTime RecordedOnUtc { get; private set; }

    public string? Note { get; private set; }
}
