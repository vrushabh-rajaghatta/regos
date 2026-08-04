using RegOS.Product.Application.Queries.ListPresentations;

namespace RegOS.Product.Application.Queries.ListProductsContainingSubstance;

/// <summary>
/// One place a substance is used: a product, in a market, in a presentation, in
/// a role, at a strength.
/// </summary>
/// <param name="MarketStatus">
/// Whether that market is actually on sale. An impact assessment cares far more
/// about a launched product than a planned one, and this is the field that
/// tells them apart — which is why the query returns it rather than leaving the
/// reader to open each market.
/// </param>
/// <param name="Role">
/// <c>Active</c> or <c>Excipient</c>. A recall over an active is a different
/// conversation from one over an excipient.
/// </param>
public sealed record SubstanceUsageDto(
    Guid GlobalProductId,
    string ProductName,
    string ProductCode,
    Guid MedicinalProductId,
    string CountryName,
    string CountryCode,
    string MarketStatus,
    Guid PresentationId,
    string PresentationName,
    CodedValueDto DoseForm,
    string Role,
    StrengthDto? Strength);
