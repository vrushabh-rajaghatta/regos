using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.Product.Application.Queries.ListMedicinalProducts;

public sealed class ListMedicinalProductsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListMedicinalProductsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns the product's markets, or null when the product does not exist —
    /// so the endpoint can 404 rather than return an empty list for a product
    /// that was never there.
    /// </summary>
    public async Task<IReadOnlyList<MedicinalProductListItem>?> HandleAsync(
        ListMedicinalProductsQuery query,
        CancellationToken cancellationToken)
    {
        var productExists = await _dbContext.Products
            .AsNoTracking()
            .AnyAsync(x => x.Id == query.GlobalProductId, cancellationToken);

        if (!productExists)
            return null;

        // Strongly-typed ids are materialised then unwrapped in memory: their
        // converters have no SQL translation for .Value.
        var rows = await (
            from market in _dbContext.MedicinalProducts.AsNoTracking()
            where market.GlobalProductId == query.GlobalProductId
            join country in _dbContext.Countries
                on market.CountryId equals country.Id
            orderby country.Name
            select new
            {
                market.Id,
                market.CountryId,
                CountryName = country.Name,
                CountryCode = country.Code,
                market.Status,
                market.StatusDate,
            }).ToListAsync(cancellationToken);

        return rows
            .Select(row => new MedicinalProductListItem(
                row.Id.Value,
                row.CountryId.Value,
                row.CountryName,
                row.CountryCode,
                row.Status.ToString(),
                row.StatusDate))
            .ToList();
    }
}
