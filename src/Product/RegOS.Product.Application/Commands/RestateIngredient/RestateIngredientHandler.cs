using RegOS.Product.Application.Services;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.RestateIngredient;

public sealed class RestateIngredientHandler
{
    private readonly IPharmaceuticalProductDetailRepository _presentations;

    public RestateIngredientHandler(
        IPharmaceuticalProductDetailRepository presentations)
    {
        _presentations = presentations;
    }

    public async Task HandleAsync(
        RestateIngredientCommand command,
        CancellationToken cancellationToken)
    {
        var presentation = await _presentations.GetByIdAsync(
                command.PresentationId, cancellationToken)
            ?? throw new NotFoundException(
                PharmaceuticalProductDetailErrors.NotFound);

        presentation.RestateIngredient(
            command.IngredientId,
            command.Role,
            StrengthFromCodes.Create(
                command.NumeratorValue,
                command.NumeratorUnitCode,
                command.DenominatorValue,
                command.DenominatorUnitCode),
            command.ManufacturingSourceSiteId);

        await _presentations.UpdateAsync(presentation, cancellationToken);
    }
}
