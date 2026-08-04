using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Queries.ListPackageItems;

public sealed class ListPackageItemsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListPackageItemsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Reads the whole pack and orders it through <see cref="PackagingTree"/>.
    /// </summary>
    /// <remarks>
    /// <b>The traversal is not repeated here</b>, and that placement is the
    /// point: a depth-first walk written in a query handler would be the second
    /// traversal of this tree in a second place, each correct on its own and
    /// collectively unreviewable. The read and the rules walk the same
    /// structure — the same call <c>ComponentTree</c> made.
    /// </remarks>
    public async Task<IReadOnlyList<PackageItemSummary>> HandleAsync(
        ListPackageItemsQuery query,
        CancellationToken cancellationToken)
    {
        var items = await _dbContext.PackageItems
            .AsNoTracking()
            .Where(x => x.PackagedProductId == query.PackagedProductId)
            .ToListAsync(cancellationToken);

        return PackagingTree.Of(items)
            .InReadingOrder()
            .Select(row => new PackageItemSummary(
                row.Item.Id.Value,
                row.Item.ParentPackageItemId?.Value,
                row.Depth,
                row.Item.ItemType.Code,
                row.Item.ItemType.Display,
                row.Item.ItemType.System,
                row.Item.Material?.Code,
                row.Item.Material?.Display,
                row.Item.Quantity,
                row.Item.UnitOfPresentation?.Code,
                row.Item.UnitOfPresentation?.Display,
                row.Item.Description))
            .ToList();
    }
}
