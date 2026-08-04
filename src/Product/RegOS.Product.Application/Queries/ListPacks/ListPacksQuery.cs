using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Queries.ListPacks;

/// <summary>
/// "What does this market sell?" — every pack of one market-local product.
/// </summary>
public sealed record ListPacksQuery(MedicinalProductId MedicinalProductId);
