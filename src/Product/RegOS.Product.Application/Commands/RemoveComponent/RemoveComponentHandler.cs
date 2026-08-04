using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.RemoveComponent;

public sealed class RemoveComponentHandler
{
    private readonly IMedicinalProductComponentRepository _components;

    public RemoveComponentHandler(
        IMedicinalProductComponentRepository components)
    {
        _components = components;
    }

    public async Task HandleAsync(
        RemoveComponentCommand command,
        CancellationToken cancellationToken)
    {
        var component = await _components.GetByIdAsync(
                command.ComponentId, cancellationToken)
            ?? throw new NotFoundException(
                MedicinalProductComponentErrors.NotFound);

        // Refuses rather than cascading. Removing a kit and silently taking
        // its contents with it is quiet data loss; emptying it first makes the
        // intent explicit, and the tree is what knows whether it is empty.
        var siblings = await _components.ListForMarketAsync(
            component.MedicinalProductId, cancellationToken);

        ComponentTree.Of(siblings).RequireNothingInside(component.Id);

        await _components.RemoveAsync(component, cancellationToken);
    }
}
