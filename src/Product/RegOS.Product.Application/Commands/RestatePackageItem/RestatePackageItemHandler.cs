using RegOS.Product.Application.Services;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.RestatePackageItem;

public sealed class RestatePackageItemHandler
{
    private readonly IPackageItemRepository _items;

    public RestatePackageItemHandler(IPackageItemRepository items)
    {
        _items = items;
    }

    public async Task HandleAsync(
        RestatePackageItemCommand command,
        CancellationToken cancellationToken)
    {
        var item = await _items.GetByIdAsync(
                command.PackageItemId, cancellationToken)
            ?? throw new NotFoundException(PackageItemErrors.NotFound);

        // No tree needed: nothing here changes the shape.
        item.Restate(
            PackVocabulary.PackageItemType(command.ItemTypeCode),
            PackVocabulary.Material(command.MaterialCode),
            command.Quantity,
            PackVocabulary.UnitOfPresentation(command.UnitOfPresentationCode),
            command.Description);

        await _items.UpdateAsync(item, cancellationToken);
    }
}
