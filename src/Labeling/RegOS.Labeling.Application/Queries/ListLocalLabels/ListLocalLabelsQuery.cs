using RegOS.Product.Domain.Product;

namespace RegOS.Labeling.Application.Queries.ListLocalLabels;

/// <summary>What labelling do we hold for this market?</summary>
public sealed record ListLocalLabelsQuery(MedicinalProductId MedicinalProductId);
