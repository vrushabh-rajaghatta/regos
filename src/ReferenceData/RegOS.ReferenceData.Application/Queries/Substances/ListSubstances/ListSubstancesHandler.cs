using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.ReferenceData.Application.Queries.Substances.ListSubstances;

public sealed class ListSubstancesHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListSubstancesHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SubstanceDto>> HandleAsync(
        ListSubstancesQuery query,
        CancellationToken cancellationToken = default)
    {
        // The shared catalogue plus this tenant's own, never another tenant's —
        // the global query filter does that, not this handler (ADR-031).
        var substances = _dbContext.Substances.AsNoTracking();

        if (query.Origin == SubstanceOrigin.Shared)
            substances = substances.Where(x => x.TenantId == null);

        if (query.Origin == SubstanceOrigin.Proprietary)
            substances = substances.Where(x => x.TenantId != null);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            // ILike rather than ToLower().Contains(): Postgres can use it, and
            // "paracetamol" should find "Paracetamol" without the caller
            // knowing how the row was capitalised.
            substances = substances.Where(x =>
                EF.Functions.ILike(x.Name, $"%{search}%")
                || (x.Inn != null && EF.Functions.ILike(x.Inn, $"%{search}%")));
        }

        return await substances
            // Shared first, then the tenant's own: the catalogue a user should
            // reach for before adding a second row for the same molecule.
            .OrderBy(x => x.TenantId != null)
            .ThenBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Select(x => new SubstanceDto(
                x.Id.Value,
                x.Name,
                x.Inn,
                new CodedConceptDto(
                    x.SubstanceClass.System,
                    x.SubstanceClass.Code,
                    x.SubstanceClass.Display),
                new CodedConceptDto(
                    x.SubstanceType.System,
                    x.SubstanceType.Code,
                    x.SubstanceType.Display),
                x.CasNumber,
                x.UniiCode,
                x.MolecularFormula,
                x.Description,
                x.TenantId == null))
            .ToListAsync(cancellationToken);
    }
}
