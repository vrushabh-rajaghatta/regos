using RegOS.Product.Application.Services;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.DescribeAppearance;

public sealed class DescribeAppearanceHandler
{
    private readonly IPharmaceuticalProductDetailRepository _presentations;

    public DescribeAppearanceHandler(
        IPharmaceuticalProductDetailRepository presentations)
    {
        _presentations = presentations;
    }

    public async Task HandleAsync(
        DescribeAppearanceCommand command,
        CancellationToken cancellationToken)
    {
        var presentation = await _presentations.GetByIdAsync(
                command.PresentationId, cancellationToken)
            ?? throw new NotFoundException(
                PharmaceuticalProductDetailErrors.NotFound);

        // Built before it is applied, so an unknown code leaves the
        // presentation exactly as it was rather than half-described.
        var appearance = PhysicalCharacteristics.Create(
            PresentationVocabulary.Colours(command.ColourCodes),
            PresentationVocabulary.Shape(command.ShapeCode),
            command.Imprint,
            command.Description);

        presentation.DescribeAppearance(appearance);

        await _presentations.UpdateAsync(presentation, cancellationToken);
    }
}
