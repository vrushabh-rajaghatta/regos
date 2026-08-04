namespace RegOS.Product.Application.Queries.ListPacks;

/// <param name="PackSizeQuantity">
/// Null with a null unit means the size is not stated — a pack in design. The
/// two are never null singly; the aggregate refuses half a size.
/// </param>
/// <param name="PackCode">
/// The market's own identifier — an NDC, a national code, a PZN. Null until the
/// market issues one.
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
    IReadOnlyList<PackMarketingStatusSummary> History);

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
