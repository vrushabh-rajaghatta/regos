using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.MedicinalProducts;

/// <param name="OccurredOn">
/// The business date this became true in the market — supplied, not taken from
/// the clock, so a carried-over portfolio can state when things happened.
/// </param>
public sealed record ChangeMarketStatusRequest(
    MarketStatus Status,
    DateOnly OccurredOn,
    string? Note = null);
