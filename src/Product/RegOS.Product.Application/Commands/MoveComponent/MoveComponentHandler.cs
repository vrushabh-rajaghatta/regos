using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.MoveComponent;

public sealed class MoveComponentHandler
{
    private readonly IMedicinalProductComponentRepository _components;

    public MoveComponentHandler(IMedicinalProductComponentRepository components)
    {
        _components = components;
    }

    public async Task HandleAsync(
        MoveComponentCommand command,
        CancellationToken cancellationToken)
    {
        var component = await _components.GetByIdAsync(
                command.ComponentId, cancellationToken)
            ?? throw new NotFoundException(
                MedicinalProductComponentErrors.NotFound);

        // The whole market's components. This is the load the cycle check is
        // only as good as: a subset could not see that the proposed parent is
        // already inside this component.
        //
        // It also settles the cross-market case without a second check — a
        // parent from another market is simply not in this tree, so the move
        // is refused as "does not exist" rather than as a special rule.
        var siblings = await _components.ListForMarketAsync(
            component.MedicinalProductId, cancellationToken);

        component.ReparentTo(
            command.NewParentComponentId, ComponentTree.Of(siblings));

        await _components.UpdateAsync(component, cancellationToken);
    }
}
