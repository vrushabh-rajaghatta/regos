using RegOS.Product.Application.Services;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.RestateComponent;

public sealed class RestateComponentHandler
{
    private readonly IMedicinalProductComponentRepository _components;

    public RestateComponentHandler(
        IMedicinalProductComponentRepository components)
    {
        _components = components;
    }

    public async Task HandleAsync(
        RestateComponentCommand command,
        CancellationToken cancellationToken)
    {
        // One component, not the tree: describing an article changes nothing
        // about the shape, so there is nothing for a tree to refuse.
        var component = await _components.GetByIdAsync(
                command.ComponentId, cancellationToken)
            ?? throw new NotFoundException(
                MedicinalProductComponentErrors.NotFound);

        component.Restate(
            ComponentVocabulary.ComponentType(command.ComponentTypeCode),
            command.Name,
            command.Description,
            command.Quantity,
            ComponentVocabulary.UnitOfPresentation(command.UnitOfPresentationCode),
            ComponentVocabulary.DoseForm(command.DoseFormCode));

        await _components.UpdateAsync(component, cancellationToken);
    }
}
