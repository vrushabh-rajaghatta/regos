using RegOS.Product.Domain.Product;

namespace RegOS.Persistence.ReadModels;

/// <summary>
/// A flat, read-only row over the Products table for the product directory.
/// </summary>
/// <remarks>
/// Bypasses the Product aggregate and its value converters for the same reason
/// UserDirectoryRow does: `product.Code.Value` cannot be translated inside a
/// predicate, so searching or filtering through the write model fails at
/// runtime rather than at compile time. Primitives keep search, filter, sort
/// and paging fully translatable.
/// </remarks>
public sealed class ProductDirectoryRow
{
    public Guid Id { get; init; }

    public Guid TenantId { get; init; }

    public string Code { get; init; } = default!;

    public string Name { get; init; } = default!;

    public ProductType Type { get; init; }

    public ProductStatus Status { get; init; }
}
