using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.ActivateMedicinalProduct;

/// <param name="On">
/// The business date this record returned to normal work.
/// </param>
public sealed record ActivateMedicinalProductCommand(
    MedicinalProductId MedicinalProductId,
    DateOnly On);
