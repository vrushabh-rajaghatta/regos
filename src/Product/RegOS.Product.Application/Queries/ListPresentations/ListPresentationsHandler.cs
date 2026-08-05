using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Queries.ListPresentations;

public sealed class ListPresentationsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListPresentationsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PresentationDto>> HandleAsync(
        ListPresentationsQuery query,
        CancellationToken cancellationToken = default)
    {
        // Only this tenant's — the global query filter does that, not this
        // handler (ADR-031). It does the same for the substances joined below:
        // the shared catalogue plus the tenant's own, never another's.
        var rows = await _dbContext.PharmaceuticalProductDetails
            .AsNoTracking()
            .Where(x => x.MedicinalProductId == query.MedicinalProductId)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.MedicinalProductId,
                x.Name,
                x.Description,
                DoseForm = new CodedValueDto(
                    x.DoseForm.System, x.DoseForm.Code, x.DoseForm.Display),
                UnitOfPresentation = x.UnitOfPresentation == null
                    ? null
                    : new CodedValueDto(
                        x.UnitOfPresentation.System,
                        x.UnitOfPresentation.Code,
                        x.UnitOfPresentation.Display),
                // Ordered here rather than left to the database. An owned
                // collection comes back in whatever order Postgres chose, and a
                // list that reshuffles between page loads reads as data
                // changing when nothing has.
                Routes = x.RoutesOfAdministration
                    .OrderBy(r => r.Display)
                    .Select(r => new CodedValueDto(r.System, r.Code, r.Display))
                    .ToList(),
                // The substance name is joined, never copied onto the
                // ingredient — the row holds an id so that renaming a substance
                // renames it everywhere it appears at once. This is the join
                // the whole epic exists to make possible, read in the
                // uninteresting direction.
                // Ordered by code rather than left to Postgres, for the same
                // reason routes are: a list that reshuffles between page loads
                // reads as data changing when nothing has.
                Colours = x.Appearance.Colours
                    .OrderBy(c => c.Code)
                    .Select(c => new CodedValueDto(c.System, c.Code, c.Display))
                    .ToList(),
                Shape = x.Appearance.Shape == null
                    ? null
                    : new CodedValueDto(
                        x.Appearance.Shape.System,
                        x.Appearance.Shape.Code,
                        x.Appearance.Shape.Display),
                Imprint = x.Appearance.Imprint,
                AppearanceDescription = x.Appearance.Description,
                Ingredients =
                    (from ingredient in x.Ingredients
                     join substance in _dbContext.Substances
                         on ingredient.SubstanceId equals substance.Id
                     orderby ingredient.Role, substance.Name
                     // Left-joined, because provenance is optional and an
                     // ingredient nobody has sourced must still appear. An
                     // inner join here would hide most of the composition.
                     join site in _dbContext.OrganizationSites
                         on ingredient.ManufacturingSourceSiteId equals site.Id
                         into sourced
                     from source in sourced.DefaultIfEmpty()
                     select new
                     {
                         ingredient.Id,
                         ingredient.SubstanceId,
                         SubstanceName = substance.Name,
                         SubstanceInn = substance.Inn,
                         ingredient.Role,
                         ingredient.Strength,
                         ingredient.ManufacturingSourceSiteId,
                         SourceName = source != null ? source.Name : null,
                     }).ToList()
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new PresentationDto(
                x.Id.Value,
                x.MedicinalProductId.Value,
                x.Name,
                x.Description,
                x.DoseForm,
                x.UnitOfPresentation,
                x.Routes,
                [.. x.Ingredients.Select(ingredient => new IngredientDto(
                    ingredient.Id.Value,
                    ingredient.SubstanceId.Value,
                    ingredient.SubstanceName,
                    ingredient.SubstanceInn,
                    ingredient.Role.ToString(),
                    Strength(ingredient.Strength),
                    ingredient.ManufacturingSourceSiteId?.Value,
                    ingredient.SourceName))],
                x.Ingredients.Any(
                    ingredient => ingredient.Role == IngredientRole.Active),
                new AppearanceDto(
                    x.Colours,
                    x.Shape,
                    x.Imprint,
                    x.AppearanceDescription,
                    x.Colours.Count > 0
                        || x.Shape is not null
                        || x.Imprint is not null
                        || x.AppearanceDescription is not null)))
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
