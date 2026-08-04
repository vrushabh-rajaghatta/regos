using RegOS.SharedKernel.Abstractions;

namespace RegOS.Product.Domain.Product;

/// <summary>
/// One dated point in a pack's commercial history: the status it held, when it
/// held it, and when RegOS learned so.
/// </summary>
/// <remarks>
/// <b>Append-only.</b> No mutating behaviour at all — the aggregate adds entries
/// and never edits or removes one. Current state lives on the pack; this records
/// how it got there.
/// <para>
/// The two dates answer different questions and must not be conflated:
/// <see cref="OccurredOn"/> is when it happened in the world, and
/// <see cref="RecordedOnUtc"/> is when RegOS learned of it. A pack discontinued
/// in 2024 and entered today says 2024.
/// </para>
/// <para>
/// <b>The third append-only status history in this shape</b>, after
/// <c>RegistrationStatusEntry</c> and <c>MarketStatusEntry</c> — whose own
/// remark named EPIC-006 as the extraction point, which passed without one being
/// made. It stays duplicated here rather than abstracted mid-slice: the shape is
/// identical, the vocabularies are not, and pulling a base type out of three
/// aggregates while adding a fourth is the speculative deletion ADR-018 forbids
/// as firmly as speculative creation. <b>Recorded so the decision is visible
/// rather than accidental.</b>
/// </para>
/// </remarks>
public sealed class PackageMarketingStatusEntry
    : Entity<PackageMarketingStatusEntryId>
{
    public const int NoteMaxLength = 500;

    // Only the PackagedProduct aggregate may record history.
    internal PackageMarketingStatusEntry(
        PackageMarketingStatusEntryId id,
        PackageMarketingStatus status,
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

    public PackageMarketingStatus Status { get; private set; }

    /// <summary>The business date — when this became true for this pack.</summary>
    public DateOnly OccurredOn { get; private set; }

    /// <summary>The system timestamp — when this was entered into RegOS.</summary>
    public DateTime RecordedOnUtc { get; private set; }

    /// <summary>
    /// Optional context a reviewer would want: "carried over from the legacy
    /// register", "artwork changeover". Free text, never parsed.
    /// </summary>
    public string? Note { get; private set; }
}
