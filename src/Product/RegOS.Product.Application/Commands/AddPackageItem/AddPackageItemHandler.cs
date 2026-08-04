using RegOS.Product.Application.Services;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Commands.AddPackageItem;

public sealed class AddPackageItemHandler
{
    private readonly IPackagedProductRepository _packs;
    private readonly IPackageItemRepository _items;
    private readonly ITenantContext _tenantContext;

    public AddPackageItemHandler(
        IPackagedProductRepository packs,
        IPackageItemRepository items,
        ITenantContext tenantContext)
    {
        _packs = packs;
        _items = items;
        _tenantContext = tenantContext;
    }

    public async Task<AddPackageItemResult> HandleAsync(
        AddPackageItemCommand command,
        CancellationToken cancellationToken)
    {
        _ = await _packs.GetByIdAsync(command.PackagedProductId, cancellationToken)
            ?? throw new NotFoundException(PackagedProductErrors.NotFound);

        // The whole pack's layers, always. A tree built from a subset would say
        // there is room where there is none, and the named parent must belong
        // to this pack — loading by pack is what makes both true rather than
        // assumed.
        var existing = await _items.ListForPackAsync(
            command.PackagedProductId, cancellationToken);

        var item = PackageItem.Create(
            _tenantContext.TenantId,
            command.PackagedProductId,
            command.ParentPackageItemId,
            PackVocabulary.PackageItemType(command.ItemTypeCode),
            PackVocabulary.Material(command.MaterialCode),
            command.Quantity,
            PackVocabulary.UnitOfPresentation(command.UnitOfPresentationCode),
            command.Description,
            PackagingTree.Of(existing));

        await _items.AddAsync(item, cancellationToken);

        return new AddPackageItemResult(item.Id);
    }
}
