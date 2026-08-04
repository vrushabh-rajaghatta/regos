using RegOS.Product.Domain.Product;

namespace RegOS.Labeling.Application.Queries.ListDrugInteractions;

/// <summary>What does this product clash with in this market?</summary>
public sealed record ListDrugInteractionsQuery(
    MedicinalProductId MedicinalProductId);
