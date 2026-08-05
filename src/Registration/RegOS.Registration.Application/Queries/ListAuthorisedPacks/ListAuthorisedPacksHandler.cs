using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.ReferenceData.Domain.Terminology;

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
    /// <para>
    /// <b>The market's country is fetched separately rather than joined in</b>,
    /// the call EPIC-022 S003 made for the same reason: it is one row asked once
    /// for the whole list, where a join would carry it down every pack. It is
    /// also the only way to hold a <c>Country</c> rather than a projection of
    /// one, and the acceptance rule is a method on <c>Country</c>.
    /// </para>
    /// </remarks>
    public async Task<MarketAuthorisedPacks> HandleAsync(
        ListAuthorisedPacksQuery query,
        CancellationToken cancellationToken)
    {
        // Reached through MedicinalProducts, which is tenant-filtered, so a
        // market this caller cannot see yields no country and no packs.
        var countryId = await _dbContext.MedicinalProducts
            .AsNoTracking()
            .Where(market => market.Id == query.MedicinalProductId)
            .Select(market => market.CountryId)
            .FirstOrDefaultAsync(cancellationToken);

        var country = await _dbContext.Countries
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == countryId, cancellationToken);

        var rows = await _dbContext.PackagedProducts
            .AsNoTracking()
            .Where(pack => pack.MedicinalProductId == query.MedicinalProductId)
            .OrderBy(pack => pack.CreatedOnUtc)
            .ThenBy(pack => pack.Id)
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
                // Deterministic: unique index on (PackagedProductId, Code).
                StorageConditions = pack.ShelfLife.StorageConditions
                    .OrderBy(condition => condition.Code)
                    .Select(condition => condition.Display)
                    .ToList(),

                // All three parts, not only the display: these are compared
                // against the market's conditions below, and CodedConcept's
                // equality is (System, Code).
                // Deterministic: unique index on (PackagedProductId, Code).
                TestedAt = pack.ShelfLife.TestedAt
                    .OrderBy(condition => condition.Code)
                    .Select(condition => new
                    {
                        condition.System,
                        condition.Code,
                        condition.Display,
                    })
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
                     orderby authorisation.AuthorisedOn, authorisation.Id
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
        var packs = rows
            .Select(row =>
            {
                var testedAt = row.TestedAt
                    .Select(x => CodedConcept.Create(x.System, x.Code, x.Display))
                    .ToList();

                return new AuthorisedPackSummary(
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
                    [.. testedAt.Select(x => x.Display)],

                    // The rule belongs to Country and is asked for rather than
                    // reimplemented: an overlap comparison written out at this
                    // call site is the one that gets forgotten when the rule
                    // stops being "any overlap".
                    country?.AcceptsStabilityDataFrom(testedAt),

                    row.LayerCount,
                    [.. row.Authorisations.Select(x => new PackAuthorisationSummary(
                        x.AuthorisationId.Value,
                        x.RegistrationId.Value,
                        x.RegistrationNumber,
                        x.CurrentStatus.ToString(),
                        x.AuthorisedOn))]);
            })
            .ToList();

        return new MarketAuthorisedPacks(
            [.. (country?.StabilityConditions ?? [])
                // Deterministic: an owned collection cannot hold one code
                // twice for a country.
                .OrderBy(x => x.Code, StringComparer.Ordinal)
                .Select(x => x.Display)],
            packs);
    }
}
