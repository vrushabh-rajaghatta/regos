namespace RegOS.Product.Application.Queries.ListPacks;

/// <param name="PackSizeQuantity">
/// Null with a null unit means the size is not stated — a pack in design. The
/// two are never null singly; the aggregate refuses half a size.
/// </param>
/// <param name="PackCode">
/// The market's own identifier — an NDC, a national code, a PZN. Null until the
/// market issues one.
/// </param>
/// <param name="LegalStatusOfSupplyCode">
/// Who may hand this pack over. Per pack rather than per product: a 16-tablet
/// pack of paracetamol may be general sale where a 100-tablet pack is
/// pharmacy-only (ADR-061 §1). Null until it is classified.
/// </param>
/// <param name="ShelfLifeValue">
/// How long it keeps, in <paramref name="ShelfLifeUnitDisplay"/>'s period. Kept
/// literal — <em>3 years</em> arrives as three years, not thirty-six months.
/// </param>
/// <param name="ShelfLifeText">
/// What the label says, in the words it was approved in — including an in-use
/// period until one is asked for structured.
/// </param>
/// <param name="StorageConditions">
/// Empty means nobody has stated any. A single
/// <c>NO_SPECIAL_PRECAUTIONS</c> means somebody checked and none are needed;
/// the two are different regulatory statements and the payload keeps them
/// distinguishable.
/// </param>
/// <param name="TestedAt">
/// The long-term conditions the shelf life was demonstrated at. <b>Not
/// <paramref name="StorageConditions"/>, which is one line up and sounds
/// alike</b> — those are label instructions, these are study conditions, and
/// only these decide whether the period holds in a given market.
/// </param>
public sealed record PackSummary(
    Guid Id,
    string Description,
    decimal? PackSizeQuantity,
    string? PackSizeUnitCode,
    string? PackSizeUnitDisplay,
    string? PackSizeUnitSystem,
    string? PackCode,
    string CurrentMarketingStatus,
    DateOnly CurrentMarketingStatusOccurredOn,
    string? LegalStatusOfSupplyCode,
    string? LegalStatusOfSupplyDisplay,
    decimal? ShelfLifeValue,
    string? ShelfLifeUnitCode,
    string? ShelfLifeUnitDisplay,
    string? ShelfLifeText,
    IReadOnlyList<PackStorageConditionSummary> StorageConditions,
    IReadOnlyList<PackTestedAtSummary> TestedAt,
    IReadOnlyList<PackMarketingStatusSummary> History);

/// <remarks>
/// <b>Read straight through, not re-ordered.</b> The value object orders its
/// conditions by code for equality only; on screen they read in the order the
/// database returns them, which is the order they were stated in.
/// </remarks>
public sealed record PackStorageConditionSummary(string Code, string Display);

/// <remarks>
/// <b>Its own record rather than a reuse of
/// <see cref="PackStorageConditionSummary"/>, which has the identical shape.</b>
/// The two lists come from different vocabularies and answer different
/// questions, and a payload that called both of them <c>StorageCondition</c>
/// would put the confusion the domain works to prevent straight back on the
/// wire. Two records is the cheaper mistake (ADR-018).
/// </remarks>
public sealed record PackTestedAtSummary(string Code, string Display);

/// <param name="RecordedOnUtc">
/// When RegOS learned of it, as against when it took effect. Kept apart because
/// both get asked about.
/// </param>
public sealed record PackMarketingStatusSummary(
    Guid Id,
    string Status,
    DateOnly OccurredOn,
    DateTime RecordedOnUtc,
    string? Note);
