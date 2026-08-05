using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.Registration.Application.Queries.ListAuthorisedPacks;

public sealed class ListAuthorisedPacksHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListAuthorisedPacksHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// <b>Starts at <c>PackagedProducts</c>, which is a filtered root</b>, and
    /// reaches authorisations and licences from it (ADR-031). Both of those
    /// carry a <c>TenantId</c> of their own, so the join cannot widen what the
    /// first filter allowed.
    /// <para>
    /// <b>Four aggregates, three contexts, and nothing duplicated.</b> The pack
    /// and its layers are Product's, the licence is Registration's, and the
    /// relationship between them is its own root — which is the shape the
    /// dependency graph forced at S001 and turned out to want anyway.
    /// </para>
    /// </remarks>
    public async Task<IReadOnlyList<AuthorisedPackSummary>> HandleAsync(
        ListAuthorisedPacksQuery query,
        CancellationToken cancellationToken)
    {
        var rows = await _dbContext.PackagedProducts
            .AsNoTracking()
            .Where(pack => pack.MedicinalProductId == query.MedicinalProductId)
            .OrderBy(pack => pack.CreatedOnUtc)
            .Select(pack => new
            {
                pack.Id,
                pack.Description,
                pack.PackSizeQuantity,
                PackSizeUnit = pack.PackSizeUnit,
                pack.PackCode,
                pack.CurrentMarketingStatus,

                LegalStatus = pack.LegalStatusOfSupply,

                pack.ShelfLife.Value,
                ShelfLifeUnit = pack.ShelfLife.Unit,
                ShelfLifeText = pack.ShelfLife.Text,

                // Ordered by code rather than left to Postgres, the same call
                // every other owned collection in this codebase makes.
                StorageConditions = pack.ShelfLife.StorageConditions
                    .OrderBy(condition => condition.Code)
                    .Select(condition => condition.Display)
                    .ToList(),

                // A count, not the tree. This read answers "is it described?";
                // /api/packaged-products/{id}/items answers "how?".
                LayerCount = _dbContext.PackageItems
                    .Count(item => item.PackagedProductId == pack.Id),

                // Joined from the authorisation, which is the only place that
                // knows both sides — and the only place carrying the date.
                Authorisations =
                    (from authorisation in _dbContext.PackAuthorisations
                     where authorisation.PackagedProductId == pack.Id
                     join registration in _dbContext.Registrations
                         on authorisation.RegistrationId equals registration.Id
                     orderby authorisation.AuthorisedOn
                     select new
                     {
                         AuthorisationId = authorisation.Id,
                         authorisation.RegistrationId,
                         registration.RegistrationNumber,
                         registration.CurrentStatus,
                         authorisation.AuthorisedOn,
                     }).ToList(),
            })
            .ToListAsync(cancellationToken);

        // Strongly-typed ids are materialised then unwrapped in memory: their
        // converters have no SQL translation for .Value.
        return rows
            .Select(row => new AuthorisedPackSummary(
                row.Id.Value,
                row.Description,
                row.PackSizeQuantity,
                row.PackSizeUnit?.Display,
                row.PackCode,
                row.CurrentMarketingStatus.ToString(),
                row.LegalStatus?.Display,
                row.Value,
                row.ShelfLifeUnit?.Display,
                row.ShelfLifeText,
                row.StorageConditions,
                row.LayerCount,
                [.. row.Authorisations.Select(x => new PackAuthorisationSummary(
                    x.AuthorisationId.Value,
                    x.RegistrationId.Value,
                    x.RegistrationNumber,
                    x.CurrentStatus.ToString(),
                    x.AuthorisedOn))]))
            .ToList();
    }
}
