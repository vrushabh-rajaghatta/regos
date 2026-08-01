namespace RegOS.Product.Domain.Product;

/// <summary>
/// One dated point in a market's commercial history: the status it held, when
/// it held it, and when RegOS learned so.
/// </summary>
/// <remarks>
/// <b>Append-only.</b> A child entity with no mutating behaviour at all — the
/// aggregate adds entries and never edits or removes one. Current state lives
/// on the medicinal product; this records how it got there.
/// <para>
/// The two dates answer different questions and must not be conflated:
/// <list type="bullet">
/// <item><see cref="OccurredOn"/> — when it happened <em>in the world</em>. A
/// product launched in 2021 and entered today says 2021.</item>
/// <item><see cref="RecordedOnUtc"/> — when <em>RegOS learned</em> of it.</item>
/// </list>
/// Storing only one of them permanently loses the ability to tell a late entry
/// from a backdated one.
/// </para>
/// <para>
/// <b>Deliberately identical in shape to <c>RegistrationStatusEntry</c></b>,
/// field for field. This is the <em>second</em> append-only status history, and
/// the Rule of Three (ADR-018) says wait — EPIC-006 brings authority-question,
/// commitment, inspection and meeting statuses, and that is the extraction
/// point. Keeping the two identical is what will make that extraction
/// mechanical rather than a reconciliation.
/// </para>
/// <para>
/// Note what does <b>not</b> duplicate: <c>RegistrationLifecycle</c>'s
/// transition table has no counterpart here. The bitemporal shape generalises;
/// the constraint graph does not, because a regulator cannot approve twice and
/// a company can absolutely launch twice.
/// </para>
/// </remarks>
public sealed class MarketStatusEntry
{
    public const int NoteMaxLength = 500;

    // Only the MedicinalProduct aggregate may record history.
    internal MarketStatusEntry(
        MarketStatusEntryId id,
        MarketStatus status,
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

    public MarketStatusEntryId Id { get; }

    public MarketStatus Status { get; }

    /// <summary>The business date — when this became true in the market.</summary>
    public DateOnly OccurredOn { get; }

    /// <summary>The system timestamp — when this was entered into RegOS.</summary>
    public DateTime RecordedOnUtc { get; }

    /// <summary>
    /// Optional context a reviewer would want: "carried over from the legacy
    /// register", "API supply interruption". Free text, never parsed.
    /// </summary>
    public string? Note { get; }
}

public readonly record struct MarketStatusEntryId(Guid Value)
{
    public static MarketStatusEntryId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
