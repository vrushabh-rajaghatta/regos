using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Geography.Country;

namespace RegOS.Product.Application.Commands.CreateMedicinalProduct;

/// <param name="StatusDate">
/// The business date this market presence began — supplied, never read from the
/// clock, so a portfolio carried over from a legacy system can state when it
/// actually entered the market.
/// </param>
public sealed record CreateMedicinalProductCommand(
    GlobalProductId GlobalProductId,
    CountryId CountryId,
    DateOnly StatusDate);
