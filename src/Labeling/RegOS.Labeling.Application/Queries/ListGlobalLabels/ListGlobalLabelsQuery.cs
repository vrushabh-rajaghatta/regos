using RegOS.Product.Domain.Product;

namespace RegOS.Labeling.Application.Queries.ListGlobalLabels;

/// <summary>What labels do we hold for this product?</summary>
public sealed record ListGlobalLabelsQuery(GlobalProductId GlobalProductId);
