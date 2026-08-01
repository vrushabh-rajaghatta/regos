using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.ChangeMarketStatus;

/// <param name="OccurredOn">
/// The business date this became true in the market. Supplied by the caller,
/// never taken from the clock, so a portfolio carried over from a legacy system
/// can state when things actually happened.
/// </param>
public sealed record ChangeMarketStatusCommand(
    MedicinalProductId MedicinalProductId,
    MarketStatus Status,
    DateOnly OccurredOn,
    string? Note = null);
