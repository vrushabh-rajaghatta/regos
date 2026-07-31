namespace RegOS.Registration.Domain.Aggregates.Registration;

/// <summary>
/// One dated point in a registration's history: the status it held, when it
/// held it, and when RegOS learned so.
/// </summary>
/// <remarks>
/// <b>Append-only.</b> A child entity with no mutating behaviour at all — the
/// aggregate adds entries and never edits or removes one. Current state lives
/// on the registration; this records how it got there, and a regulated record
/// whose history can be rewritten is not a history.
/// <para>
/// The two dates answer different questions and must not be conflated:
/// <list type="bullet">
/// <item><see cref="OccurredOn"/> — when it happened <em>in the world</em>. A
/// registration granted in 2019 and entered today says 2019.</item>
/// <item><see cref="RecordedOnUtc"/> — when <em>RegOS learned</em> of it.</item>
/// </list>
/// Storing only one of them permanently loses the ability to tell a late entry
/// from a backdated one.
/// </para>
/// <para>
/// Entries are statuses, not a parallel event vocabulary: the history is a
/// chronological sequence of the states the registration has held, so there is
/// one word for a thing rather than two.
/// </para>
/// </remarks>
public sealed class RegistrationStatusEntry
{
    // Only the Registration aggregate may record history.
    internal RegistrationStatusEntry(
        RegistrationStatusEntryId id,
        RegistrationStatus status,
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

    public RegistrationStatusEntryId Id { get; }

    public RegistrationStatus Status { get; }

    /// <summary>The business date — when the authority's decision took effect.</summary>
    public DateOnly OccurredOn { get; }

    /// <summary>The system timestamp — when this was entered into RegOS.</summary>
    public DateTime RecordedOnUtc { get; }

    /// <summary>
    /// Optional context a reviewer would want: "carried over from the legacy
    /// register", "renewal granted". Free text, never parsed.
    /// </summary>
    public string? Note { get; }

    public const int NoteMaxLength = 500;
}

public readonly record struct RegistrationStatusEntryId(Guid Value)
{
    public static RegistrationStatusEntryId New()
        => new(Guid.NewGuid());

    public override string ToString()
        => Value.ToString();
}
