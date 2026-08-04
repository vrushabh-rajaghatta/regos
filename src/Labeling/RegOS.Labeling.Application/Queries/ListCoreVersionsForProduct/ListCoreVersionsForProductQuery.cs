using RegOS.Product.Domain.Product;

namespace RegOS.Labeling.Application.Queries.ListCoreVersionsForProduct;

/// <summary>
/// Every core-label version a market could say it was written from.
/// </summary>
/// <remarks>
/// Flattened across the product's global labels on purpose. The question a
/// person asks while preparing a Japanese revision is <em>"which core version is
/// this?"</em> — not <em>"which core label, and then which version of it"</em> —
/// and a screen that made them pick twice would be exposing our aggregate
/// boundaries as a workflow.
/// <para>
/// It includes superseded versions: a market catching up may be adopting a core
/// version the company has already moved past, which is the ordinary case
/// rather than an error.
/// </para>
/// </remarks>
public sealed record ListCoreVersionsForProductQuery(
    GlobalProductId GlobalProductId);
