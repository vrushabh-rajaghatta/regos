using RegOS.Product.Application.Services;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.RestatePresentation;

public sealed class RestatePresentationHandler
{
    private readonly IPharmaceuticalProductDetailRepository _presentations;

    public RestatePresentationHandler(
        IPharmaceuticalProductDetailRepository presentations)
    {
        _presentations = presentations;
    }

    public async Task HandleAsync(
        RestatePresentationCommand command,
        CancellationToken cancellationToken)
    {
        // No tenant check here: the repository reads through the fail-closed
        // query filter, so another tenant's presentation is not found rather
        // than refused (ADR-031).
        var presentation = await _presentations.GetByIdAsync(
                command.PresentationId, cancellationToken)
            ?? throw new NotFoundException(
                PharmaceuticalProductDetailErrors.NotFound);

        presentation.Restate(
            command.Name,
            command.Description,
            PresentationVocabulary.DoseForm(command.DoseFormCode),
            PresentationVocabulary.UnitOfPresentation(
                command.UnitOfPresentationCode),
            PresentationVocabulary.Routes(command.RouteCodes));

        await _presentations.UpdateAsync(presentation, cancellationToken);
    }
}
