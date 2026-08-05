using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Queries.ListPackageItems;

/// <summary>
/// "What is inside this pack?" — every layer, in reading order.
/// </summary>
public sealed record ListPackageItemsQuery(PackagedProductId PackagedProductId);
