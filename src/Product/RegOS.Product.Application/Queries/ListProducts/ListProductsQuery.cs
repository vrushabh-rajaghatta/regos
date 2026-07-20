using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Queries.ListProducts;

/// <summary>
/// Browses the product directory for the caller's tenant. The tenant is
/// ambient, so it is not a parameter and cannot be widened by omitting it.
/// <paramref name="Search"/> matches code or name.
/// </summary>
public sealed record ListProductsQuery(
    string? Search = null,
    ProductType? Type = null,
    ProductStatus? Status = null,
    int Page = ListProductsQuery.DefaultPage,
    int PageSize = ListProductsQuery.DefaultPageSize)
{
    public const int DefaultPage = 1;

    public const int DefaultPageSize = 20;

    /// <summary>Unbounded reads are never allowed.</summary>
    public const int MaxPageSize = 100;
}
