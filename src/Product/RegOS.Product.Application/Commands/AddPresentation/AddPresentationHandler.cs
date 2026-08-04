using RegOS.Product.Application.Services;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.AddPresentation;

public sealed class AddPresentationHandler
{
    private readonly IMedicinalProductRepository _markets;
    private readonly IPharmaceuticalProductDetailRepository _presentations;
    private readonly ITenantContext _tenantContext;

    public AddPresentationHandler(
        IMedicinalProductRepository markets,
        IPharmaceuticalProductDetailRepository presentations,
        ITenantContext tenantContext)
    {
        _markets = markets;
        _presentations = presentations;
        _tenantContext = tenantContext;
    }

    public async Task<AddPresentationResult> HandleAsync(
        AddPresentationCommand command,
        CancellationToken cancellationToken)
    {
        // The market is loaded to prove it exists and is this tenant's, not to
        // change it — a presentation is its own aggregate and its own
        // transaction. Without this, a wrong id would create an orphan the
        // foreign key would only reject at save time, as a 500 rather than a
        // 404.
        _ = await _markets.GetByIdAsync(
                command.MedicinalProductId, cancellationToken)
            ?? throw new NotFoundException(
                MedicinalProductPolicyErrors.MedicinalProductDoesNotExist);

        var presentation = PharmaceuticalProductDetail.Create(
            _tenantContext.TenantId,
            command.MedicinalProductId,
            command.Name,
            command.Description,
            PresentationVocabulary.DoseForm(command.DoseFormCode),
            PresentationVocabulary.UnitOfPresentation(
                command.UnitOfPresentationCode),
            PresentationVocabulary.Routes(command.RouteCodes));

        await _presentations.AddAsync(presentation, cancellationToken);

        return new AddPresentationResult(presentation.Id);
    }
}
