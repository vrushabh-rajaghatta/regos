namespace RegOS.Interaction.Domain.Correspondence;

/// <summary>
/// One dated point in a question's history: the status it held, when it held
/// it, and when RegOS learned so.
/// </summary>
/// <remarks>
/// <b>Append-only.</b> No mutating behaviour at all — the question adds entries
/// and never edits or removes one.
/// <para>
/// The two dates answer different questions and must not be conflated:
/// <see cref="OccurredOn"/> is when it happened in the world, and
/// <see cref="RecordedOnUtc"/> is when RegOS learned. Storing only one loses
/// the ability to tell a late entry from a backdated one.
/// </para>
/// <para>
/// <b>The third copy, written by hand on purpose.</b> It is field-for-field
/// identical to <c>RegistrationStatusEntry</c> and <c>MarketStatusEntry</c>, and
/// ADR-039 named EPIC-006 as the extraction point. The Rule of Three asks for
/// three <em>demonstrated</em> consumers, not the third consumer — so this one
/// is written out so the extraction can be argued from measurements rather than
/// from a prediction. See the S003 extraction review in the epic.
/// </para>
/// </remarks>
public sealed class HaQuestionStatusEntry
{
    public const int NoteMaxLength = 500;

    // Only HaQuestion may record history.
    internal HaQuestionStatusEntry(
        HaQuestionStatusEntryId id,
        HaQuestionStatus status,
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

    public HaQuestionStatusEntryId Id { get; }

    public HaQuestionStatus Status { get; }

    /// <summary>The business date — when this became true.</summary>
    public DateOnly OccurredOn { get; }

    /// <summary>The system timestamp — when this was entered into RegOS.</summary>
    public DateTime RecordedOnUtc { get; }

    /// <summary>Optional context a reviewer would want. Free text, never parsed.</summary>
    public string? Note { get; }
}

public readonly record struct HaQuestionStatusEntryId(Guid Value)
{
    public static HaQuestionStatusEntryId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
