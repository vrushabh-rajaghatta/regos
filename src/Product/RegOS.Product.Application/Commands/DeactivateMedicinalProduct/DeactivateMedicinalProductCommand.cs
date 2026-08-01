using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.DeactivateMedicinalProduct;

/// <param name="On">
/// The business date this record left normal work. Supplied, never taken from
/// the clock — the same discipline every other date in this context follows.
/// </param>
public sealed record DeactivateMedicinalProductCommand(
    MedicinalProductId MedicinalProductId,
    DateOnly On);
