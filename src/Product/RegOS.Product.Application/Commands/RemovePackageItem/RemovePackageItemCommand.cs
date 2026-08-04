using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.RemovePackageItem;

public sealed record RemovePackageItemCommand(PackageItemId PackageItemId);
