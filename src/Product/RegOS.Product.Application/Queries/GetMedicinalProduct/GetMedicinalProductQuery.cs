using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Queries.GetMedicinalProduct;

public sealed record GetMedicinalProductQuery(
    MedicinalProductId MedicinalProductId);
