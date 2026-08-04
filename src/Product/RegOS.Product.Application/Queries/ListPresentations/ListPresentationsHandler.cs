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
        // handler (ADR-031).
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
                // route list that reshuffles between page loads reads as data
                // changing when nothing has.
                Routes = x.RoutesOfAdministration
                    .OrderBy(r => r.Display)
                    .Select(r => new CodedValueDto(r.System, r.Code, r.Display))
                    .ToList()
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
                x.Routes))
            .ToList();
    }
}
