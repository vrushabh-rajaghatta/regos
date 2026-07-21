using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Application.Common;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Abstractions;

namespace RegOS.Product.Application.Queries.ListProducts;

/// <summary>
/// Reads the product directory straight from the database. This is reporting,
/// not domain modelling: no repository, no aggregate loading, no tracking —
/// only the columns the directory screen needs, projected from a flat read
/// model rather than through the Product aggregate's value converters.
/// </summary>
public sealed class ListProductsHandler
{
    private readonly RegOSDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public ListProductsHandler(
        RegOSDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<PagedResult<ProductListItem>> HandleAsync(
        ListProductsQuery query,
        CancellationToken cancellationToken)
    {
        // Clamp rather than reject: a caller asking for page 0 or 5000 rows
        // gets a sensible page, never an unbounded read.
        var page = query.Page < 1 ? ListProductsQuery.DefaultPage : query.Page;
        var pageSize = Math.Clamp(
            query.PageSize, 1, ListProductsQuery.MaxPageSize);

        var tenantId = _tenantContext.TenantId.Value;

        // Tenant filter first and unconditionally — there is no branch that can
        // skip it.
        var products = _dbContext.ProductDirectory
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (query.Type is not null)
        {
            var type = query.Type.Value;
            products = products.Where(x => x.Type == type);
        }

        if (query.Status is not null)
        {
            var status = query.Status.Value;
            products = products.Where(x => x.Status == status);
        }
        else
        {
            // Archived products are retired from new work, so the directory
            // hides them by default. They are not deleted: asking for
            // ?status=Archived still lists them, and their detail page and
            // every existing reference keep working.
            products = products.Where(
                x => x.Status != ProductStatus.Archived);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            // One search box across code and name.
            var pattern = $"%{query.Search.Trim()}%";

            products = products.Where(x =>
                EF.Functions.ILike(x.Code, pattern)
                || EF.Functions.ILike(x.Name, pattern));
        }

        var totalCount = await products.CountAsync(cancellationToken);

        var items = await products
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ProductListItem(
                x.Id,
                x.Code,
                x.Name,
                x.Type,
                x.Status))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductListItem>(
            items, totalCount, page, pageSize);
    }
}
