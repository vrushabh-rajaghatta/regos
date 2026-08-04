using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.RestateIngredient;

/// <remarks>
/// No substance: a different substance is a different ingredient, so swapping
/// one is add-then-remove. Offering it here would leave no way to tell a
/// correction from a replacement.
/// </remarks>
public sealed record RestateIngredientCommand(
    PharmaceuticalProductDetailId PresentationId,
    IngredientId IngredientId,
    IngredientRole Role,
    decimal? NumeratorValue,
    string? NumeratorUnitCode,
    decimal? DenominatorValue,
    string? DenominatorUnitCode);
