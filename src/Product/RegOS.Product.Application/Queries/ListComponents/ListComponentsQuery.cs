using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Queries.ListComponents;

/// <summary>
/// "What does the patient actually receive?" — every article in one market,
/// flat, with each one saying what holds it.
/// </summary>
/// <remarks>
/// <b>Flat, not nested.</b> The tree is at most three levels and a handful of
/// rows, so shaping it into JSON nesting on the server would buy nothing and
/// commit the API to one traversal. The client assembles it from
/// <c>parentComponentId</c>, which is also the form a future recursive CTE
/// would return.
/// </remarks>
public sealed record ListComponentsQuery(MedicinalProductId MedicinalProductId);
