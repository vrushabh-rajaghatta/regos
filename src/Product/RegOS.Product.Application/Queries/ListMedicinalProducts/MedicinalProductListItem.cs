namespace RegOS.Product.Application.Queries.ListMedicinalProducts;

/// <summary>
/// One market this product is present in, and what it is called there.
/// </summary>
/// <remarks>
/// Deliberately does not count the registrations held there. The count is a
/// Registration fact, and a Product query reaching into another context's
/// tables to compute it would be the cheapest possible way to couple them —
/// the caller already has the registration list keyed by medicinal product.
/// </remarks>
public sealed record MedicinalProductListItem(
    Guid MedicinalProductId,
    Guid CountryId,
    string CountryName,
    string CountryCode,
    string Status,
    DateOnly StatusDate,
    IReadOnlyList<TradeNameListItem> TradeNames);

/// <param name="Language">An ISO 639-1 code. The screen renders the name.</param>
public sealed record TradeNameListItem(
    Guid TradeNameId,
    string Language,
    string Name);
