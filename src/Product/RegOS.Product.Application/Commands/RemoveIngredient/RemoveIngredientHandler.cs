using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.RemoveIngredient;

public sealed class RemoveIngredientHandler
{
    private readonly IPharmaceuticalProductDetailRepository _presentations;

    public RemoveIngredientHandler(
        IPharmaceuticalProductDetailRepository presentations)
    {
        _presentations = presentations;
    }

    public async Task HandleAsync(
        RemoveIngredientCommand command,
        CancellationToken cancellationToken)
    {
        // The whole composition is loaded, not just the row: the aggregate has
        // to see the other ingredients to know whether removing this one would
        // leave a formulation with excipients and no active.
        var presentation = await _presentations.GetByIdAsync(
                command.PresentationId, cancellationToken)
            ?? throw new NotFoundException(
                PharmaceuticalProductDetailErrors.NotFound);

        presentation.RemoveIngredient(command.IngredientId);

        await _presentations.UpdateAsync(presentation, cancellationToken);
    }
}
