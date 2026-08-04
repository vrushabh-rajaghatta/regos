using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Application.Queries.ListPresentations;
using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Queries.ListProductsContainingSubstance;

/// <summary>
/// The capstone read: every product that contains one substance, across every
/// market.
/// </summary>
/// <remarks>
/// <b>It lives in <c>Product</c> rather than <c>ReferenceData</c>, and the
/// dependency graph decides that.</b> <c>Product → ReferenceData</c> is an
/// established edge; the reverse is not, and putting this beside
/// <c>Substance</c> would invert it for a read. The same reasoning ADR-058 §3
/// used to place <c>CodedConcept</c>, and the same shape EPIC-019 used for
/// <c>ListStudyFilings</c>: <b>a real question spanning two contexts is a read,
/// and a read grants nobody write ownership.</b>
/// <para>
/// <b>The walk starts at the presentation, not at the ingredient, and that is a
/// tenant-isolation decision rather than a stylistic one.</b> A query filter
/// applies per entity type — <c>Ingredient</c> is a child, carries no
/// <c>TenantId</c> and therefore has no filter of its own. Querying
/// <c>Set&lt;Ingredient&gt;()</c> directly would read every tenant's
/// compositions. Starting from <c>PharmaceuticalProductDetails</c>, which is
/// fail-closed filtered, is what confines the whole join to the caller
/// (ADR-031).
/// </para>
/// </remarks>
public sealed class ListProductsContainingSubstanceHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListProductsContainingSubstanceHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SubstanceUsageDto>> HandleAsync(
        ListProductsContainingSubstanceQuery query,
        CancellationToken cancellationToken = default)
    {
        var rows = await (
            // Every hop is a join on an id, which is what the split into
            // Substance and Ingredient bought: no step here matches on a name.
            from presentation in _dbContext.PharmaceuticalProductDetails.AsNoTracking()
            from ingredient in presentation.Ingredients
            where ingredient.SubstanceId == query.SubstanceId
            join market in _dbContext.MedicinalProducts
                on presentation.MedicinalProductId equals market.Id
            join product in _dbContext.Products
                on market.GlobalProductId equals product.Id
            join country in _dbContext.Countries
                on market.CountryId equals country.Id
            select new
            {
                market.GlobalProductId,
                ProductName = product.Name,
                ProductCode = product.Code,
                MedicinalProductId = market.Id,
                CountryName = country.Name,
                CountryCode = country.Code,
                market.CurrentMarketStatus,
                PresentationId = presentation.Id,
                PresentationName = presentation.Name,
                presentation.DoseForm,
                ingredient.Role,
                ingredient.Strength
            }).ToListAsync(cancellationToken);

        // Ordered by what a reader scans for: the product first, then where it
        // is sold, then which presentation. Sorting by market alone would
        // scatter one product's entries across the list.
        return rows
            .OrderBy(x => x.ProductName.Value, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.CountryName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.PresentationName, StringComparer.OrdinalIgnoreCase)
            .Select(x => new SubstanceUsageDto(
                x.GlobalProductId.Value,
                x.ProductName.Value,
                x.ProductCode.Value,
                x.MedicinalProductId.Value,
                x.CountryName,
                x.CountryCode,
                x.CurrentMarketStatus.ToString(),
                x.PresentationId.Value,
                x.PresentationName,
                new CodedValueDto(
                    x.DoseForm.System, x.DoseForm.Code, x.DoseForm.Display),
                x.Role.ToString(),
                Strength(x.Strength)))
            .ToList();
    }

    private static StrengthDto? Strength(Strength? strength)
        => strength is null
            ? null
            : new StrengthDto(
                strength.NumeratorValue,
                new CodedValueDto(
                    strength.NumeratorUnit.System,
                    strength.NumeratorUnit.Code,
                    strength.NumeratorUnit.Display),
                strength.DenominatorValue,
                strength.DenominatorUnit is null
                    ? null
                    : new CodedValueDto(
                        strength.DenominatorUnit.System,
                        strength.DenominatorUnit.Code,
                        strength.DenominatorUnit.Display));
}
