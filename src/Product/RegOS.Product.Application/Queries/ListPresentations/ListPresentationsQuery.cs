using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Queries.ListPresentations;

/// <summary>
/// "What is this product, in this market?" — every presentation the market has.
/// </summary>
/// <remarks>
/// Its own query rather than a field on <c>GetMedicinalProduct</c>. The market
/// and its presentations are separate aggregates with separate lifecycles, and
/// folding one into the other's read would put composition on the critical path
/// of every market load — including the ones that only wanted a trade name.
/// </remarks>
public sealed record ListPresentationsQuery(MedicinalProductId MedicinalProductId);
