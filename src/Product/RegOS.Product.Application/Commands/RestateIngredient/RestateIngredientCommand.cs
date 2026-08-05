using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.RestateIngredient;

/// <remarks>
/// No substance: a different substance is a different ingredient, so swapping
/// one is add-then-remove. Offering it here would leave no way to tell a
/// correction from a replacement.
/// </remarks>
/// <param name="ManufacturingSourceSiteId">
/// <b>Where this substance comes from</b> — a different stage of the supply
/// chain from the site that makes the finished product (ADR-063 §2). Sent on every restate, because the aggregate
/// takes no default here: a defaulted null would silently erase provenance.
/// </param>
public sealed record RestateIngredientCommand(
    PharmaceuticalProductDetailId PresentationId,
    IngredientId IngredientId,
    IngredientRole Role,
    decimal? NumeratorValue,
    string? NumeratorUnitCode,
    decimal? DenominatorValue,
    string? DenominatorUnitCode,
    OrganizationSiteId? ManufacturingSourceSiteId);
