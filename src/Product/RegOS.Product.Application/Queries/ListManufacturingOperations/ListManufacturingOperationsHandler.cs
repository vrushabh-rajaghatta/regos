using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.Product.Application.Queries.ListManufacturingOperations;

public sealed class ListManufacturingOperationsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListManufacturingOperationsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// <b>Starts at <c>ManufacturingOperations</c>, which is a filtered root</b>
    /// (ADR-031). The site it joins carries a <c>TenantId</c> of its own, so the
    /// join cannot widen what the first filter allowed.
    /// <para>
    /// <b>The site's name and identifiers are joined, never copied.</b> There is
    /// no <c>ManufacturerName</c> column anywhere in RegOS — a copied name is a
    /// second place for the truth to live, and the first thing to go stale when
    /// a plant is renamed (ADR-063 §3).
    /// </para>
    /// <para>
    /// <b>Current first, then most recent.</b> Somebody asking where a product
    /// is made wants today's answer at the top; the closed periods below it are
    /// what makes a 2023 filing explainable.
    /// </para>
    /// </remarks>
    public async Task<IReadOnlyList<ManufacturingOperationSummary>> HandleAsync(
        ListManufacturingOperationsQuery query,
        CancellationToken cancellationToken)
    {
        var rows = await (
            from operation in _dbContext.ManufacturingOperations.AsNoTracking()
            where operation.MedicinalProductId == query.MedicinalProductId
            join site in _dbContext.OrganizationSites
                on operation.OrganizationSiteId equals site.Id
            join country in _dbContext.Countries
                on site.Address.CountryId equals country.Id
            // **Tie-broken, and the tie is ordinary rather than exotic.** Two
            // operations at one site starting the same day — a plant that
            // manufactures and releases — collide on both keys above, and
            // Postgres is free to return them either way round. Left there,
            // the list reorders itself between reloads for no reason a user
            // can see. Found by a browser spec taking "the first row" and
            // getting a different one on one run in thirty.
            orderby operation.CeasedOn == null descending,
                operation.EffectiveFrom descending,
                site.Name,
                operation.Operation.Code, operation.Id
            select new
            {
                OperationId = operation.Id,
                SiteId = site.Id,
                SiteName = site.Name,
                CountryName = country.Name,
                site.Type,
                Identifiers = site.Identifiers
                    .OrderBy(identifier => identifier.Value)
                    .ThenBy(identifier => identifier.Id)
                    .Select(identifier => identifier.Value)
                    .ToList(),
                operation.Operation.Code,
                operation.Operation.Display,
                operation.EffectiveFrom,
                operation.CeasedOn,
            })
            .ToListAsync(cancellationToken);

        // Strongly-typed ids are materialised then unwrapped in memory: their
        // converters have no SQL translation for .Value.
        return rows
            .Select(row => new ManufacturingOperationSummary(
                row.OperationId.Value,
                row.SiteId.Value,
                row.SiteName,
                row.CountryName,
                row.Type.ToString(),
                row.Identifiers,
                row.Code,
                row.Display,
                row.EffectiveFrom,
                row.CeasedOn,
                row.CeasedOn is null))
            .ToList();
    }
}
