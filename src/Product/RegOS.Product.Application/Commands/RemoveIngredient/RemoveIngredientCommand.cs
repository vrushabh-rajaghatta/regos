using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.RemoveIngredient;

public sealed record RemoveIngredientCommand(
    PharmaceuticalProductDetailId PresentationId,
    IngredientId IngredientId);
