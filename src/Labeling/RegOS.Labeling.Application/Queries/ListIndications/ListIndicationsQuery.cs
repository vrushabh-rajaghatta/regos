using RegOS.Product.Domain.Product;

namespace RegOS.Labeling.Application.Queries.ListIndications;

/// <summary>What is this product approved to treat in this market?</summary>
public sealed record ListIndicationsQuery(MedicinalProductId MedicinalProductId);
