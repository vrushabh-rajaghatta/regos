using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.RemovePackageItem;

public sealed class RemovePackageItemHandler
{
    private readonly IPackageItemRepository _items;

    public RemovePackageItemHandler(IPackageItemRepository items)
    {
        _items = items;
    }

    public async Task HandleAsync(
        RemovePackageItemCommand command,
        CancellationToken cancellationToken)
    {
        var item = await _items.GetByIdAsync(
                command.PackageItemId, cancellationToken)
            ?? throw new NotFoundException(PackageItemErrors.NotFound);

        var existing = await _items.ListForPackAsync(
            item.PackagedProductId, cancellationToken);

        // Refused rather than cascaded: removing a carton that still holds
        // blisters would silently take them with it, and the aggregate answers
        // that out loud instead.
        PackagingTree.Of(existing).RequireNothingInside(item.Id);

        await _items.RemoveAsync(item, cancellationToken);
    }
}
