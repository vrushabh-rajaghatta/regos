using RegOS.Product.Application.Services;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.AddComponent;

public sealed class AddComponentHandler
{
    private readonly IMedicinalProductRepository _markets;
    private readonly IMedicinalProductComponentRepository _components;
    private readonly ITenantContext _tenantContext;

    public AddComponentHandler(
        IMedicinalProductRepository markets,
        IMedicinalProductComponentRepository components,
        ITenantContext tenantContext)
    {
        _markets = markets;
        _components = components;
        _tenantContext = tenantContext;
    }

    public async Task<AddComponentResult> HandleAsync(
        AddComponentCommand command,
        CancellationToken cancellationToken)
    {
        _ = await _markets.GetByIdAsync(
                command.MedicinalProductId, cancellationToken)
            ?? throw new NotFoundException(
                MedicinalProductPolicyErrors.MedicinalProductDoesNotExist);

        // The whole market's components, always. A tree built from a subset
        // would say there is room where there is none, and the named parent
        // must be in this market — loading by market is what makes both true
        // rather than assumed.
        var existing = await _components.ListForMarketAsync(
            command.MedicinalProductId, cancellationToken);

        var component = MedicinalProductComponent.Create(
            _tenantContext.TenantId,
            command.MedicinalProductId,
            command.ParentComponentId,
            ComponentVocabulary.ComponentType(command.ComponentTypeCode),
            command.Name,
            command.Description,
            command.Quantity,
            ComponentVocabulary.UnitOfPresentation(command.UnitOfPresentationCode),
            ComponentVocabulary.DoseForm(command.DoseFormCode),
            ComponentTree.Of(existing));

        await _components.AddAsync(component, cancellationToken);

        return new AddComponentResult(component.Id);
    }
}
