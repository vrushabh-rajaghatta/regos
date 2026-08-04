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
/// <param name="AtcCode">
/// As the tenant supplied it, and not verified: RegOS holds no WHO ATC index
/// (ADR-058 §6). Sent as a plain string because that is the whole of the claim.
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
    string? AtcCode,
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
