namespace RegOS.Product.Application.Queries.ListMedicinalProducts;

/// <summary>
/// One market this product is present in, what it is called there, and whether
/// it is actually on sale.
/// </summary>
/// <remarks>
/// Deliberately does not count the registrations held there. The count is a
/// Registration fact, and a Product query reaching into another context's
/// tables to compute it would be the cheapest possible way to couple them —
/// the caller already has the registration list keyed by medicinal product.
/// </remarks>
/// <param name="Status">
/// Whether this <em>record</em> is in use — not whether the product is on sale.
/// </param>
/// <param name="MarketStatus">
/// Whether the product is on sale. A different question from
/// <paramref name="Status"/>, and the two never merge.
/// </param>
/// <param name="LaunchedOn">
/// <b>Derived, never stored.</b> The business date of the first entry reaching
/// <c>Launched</c> — so it cannot disagree with the history, and cannot precede
/// an approval that a user typed it before. Null while a market has never
/// launched. First rather than most recent: a relaunch is a different question,
/// and "when did we launch" means the original (ADR-037 — persist regulatory
/// facts, derive regulatory interpretation).
/// </param>
public sealed record MedicinalProductListItem(
    Guid MedicinalProductId,
    Guid CountryId,
    string CountryName,
    string CountryCode,
    string Status,
    DateOnly StatusDate,
    string MarketStatus,
    DateOnly? LaunchedOn,
    IReadOnlyList<TradeNameListItem> TradeNames);

/// <param name="Language">An ISO 639-1 code. The screen renders the name.</param>
public sealed record TradeNameListItem(
    Guid TradeNameId,
    string Language,
    string Name);
