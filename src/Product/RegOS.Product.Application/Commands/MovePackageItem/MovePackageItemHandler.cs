using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.MovePackageItem;

public sealed class MovePackageItemHandler
{
    private readonly IPackageItemRepository _items;

    public MovePackageItemHandler(IPackageItemRepository items)
    {
        _items = items;
    }

    public async Task HandleAsync(
        MovePackageItemCommand command,
        CancellationToken cancellationToken)
    {
        var item = await _items.GetByIdAsync(
                command.PackageItemId, cancellationToken)
            ?? throw new NotFoundException(PackageItemErrors.NotFound);

        // Every layer of the pack: the acyclicity check walks ancestors and the
        // depth check measures the moved layer's own height, and both are
        // optimistic against a partial load.
        var existing = await _items.ListForPackAsync(
            item.PackagedProductId, cancellationToken);

        item.MoveTo(
            command.NewParentPackageItemId, PackagingTree.Of(existing));

        await _items.UpdateAsync(item, cancellationToken);
    }
}
