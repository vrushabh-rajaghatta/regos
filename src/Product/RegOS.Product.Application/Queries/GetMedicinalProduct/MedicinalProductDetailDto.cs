namespace RegOS.Product.Application.Queries.GetMedicinalProduct;

/// <param name="Status">
/// Whether this <em>record</em> participates in normal work — not whether the
/// product is on sale.
/// </param>
/// <param name="MarketStatus">
/// Whether the product is on sale. A different question from
/// <paramref name="Status"/>, and the two never merge.
/// </param>
/// <param name="LaunchedOn">
/// Derived from <paramref name="MarketStatusHistory"/>, never stored — the
/// business date of the first entry reaching <c>Launched</c>.
/// </param>
public sealed record MedicinalProductDetailDto(
    Guid MedicinalProductId,
    Guid GlobalProductId,
    string ProductName,
    string ProductCode,
    Guid CountryId,
    string CountryName,
    string CountryCode,
    string Status,
    DateOnly StatusDate,
    string MarketStatus,
    DateOnly? LaunchedOn,
    IReadOnlyList<TradeNameDto> TradeNames,
    IReadOnlyList<MarketStatusEntryDto> MarketStatusHistory);

public sealed record TradeNameDto(
    Guid TradeNameId,
    string Language,
    string Name);

/// <param name="OccurredOn">When it became true in the market.</param>
/// <param name="RecordedOnUtc">When RegOS learned of it.</param>
public sealed record MarketStatusEntryDto(
    Guid Id,
    string Status,
    DateOnly OccurredOn,
    DateTime RecordedOnUtc,
    string? Note);
