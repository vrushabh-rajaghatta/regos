using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Application.Queries.ListPresentations;
using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Queries.ListComponents;

public sealed class ListComponentsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListComponentsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ComponentDto>> HandleAsync(
        ListComponentsQuery query,
        CancellationToken cancellationToken = default)
    {
        // Only this tenant's — the global query filter does that, not this
        // handler (ADR-031).
        var components = await _dbContext.MedicinalProductComponents
            .AsNoTracking()
            .Where(x => x.MedicinalProductId == query.MedicinalProductId)
            .ToListAsync(cancellationToken);

        // The same ComponentTree the write path uses, walked the same way. The
        // depth of a row is a fact about the tree, and computing it a second
        // way here is how two answers start to disagree.
        return ComponentTree.Of(components)
            .InReadingOrder()
            .Select(row => Dto(row.Component, row.Depth))
            .ToList();
    }

    private static ComponentDto Dto(MedicinalProductComponent component, int depth)
        => new(
            component.Id.Value,
            component.MedicinalProductId.Value,
            component.ParentComponentId?.Value,
            depth,
            new CodedValueDto(
                component.ComponentType.System,
                component.ComponentType.Code,
                component.ComponentType.Display),
            component.Name,
            component.Description,
            component.Quantity,
            Coded(component.UnitOfPresentation),
            Coded(component.DoseForm));

    private static CodedValueDto? Coded(
        ReferenceData.Domain.Terminology.CodedConcept? concept)
        => concept is null
            ? null
            : new CodedValueDto(concept.System, concept.Code, concept.Display);
}
