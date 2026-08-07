using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.Registration.Domain.Aggregates.Registration;

using RegistrationAggregate = RegOS.Registration.Domain.Aggregates.Registration.Registration;

namespace RegOS.Registration.Application.Queries.ListMarketRegistrations;

/// <summary>
/// "What do we hold in this market?" — the second half of the portfolio
/// question, the mirror of <c>ListProductRegistrations</c>.
/// </summary>
public sealed class ListMarketRegistrationsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListMarketRegistrationsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns everything held in the market, or null when the country does not
    /// exist — so the endpoint can 404 rather than return an empty list for a
    /// country that was never there.
    /// </summary>
    /// <remarks>
    /// Every registration, whatever its status: a withdrawn authorisation is
    /// still part of the regulatory portfolio, and "what do we hold" is not the
    /// same question as "what is currently marketable". Narrowing that is
    /// presentation, and belongs to the client — filtering here would be data
    /// loss dressed as a default.
    /// </remarks>
    public async Task<IReadOnlyList<MarketRegistrationSummary>?> HandleAsync(
        CountryId countryId,
        CancellationToken cancellationToken)
    {
        var countryExists = await _dbContext.Countries
            .AsNoTracking()
            .AnyAsync(x => x.Id == countryId, cancellationToken);

        if (!countryExists)
            return null;

        // Strongly-typed ids are materialised then unwrapped in memory: their
        // converters have no SQL translation for .Value.
        var rows = await (
            from registration in _dbContext.Set<RegistrationAggregate>()
                .AsNoTracking()
            join market in _dbContext.MedicinalProducts
                on registration.MedicinalProductId equals market.Id
            where market.CountryId == countryId
            join product in _dbContext.Products
                on market.GlobalProductId equals product.Id
            join authority in _dbContext.Authorities
                on registration.AuthorityId equals authority.Id
            join holder in _dbContext.Organizations
                on registration.HolderOrganizationId equals holder.Id
            // BUG-001. The tie-breaker belongs HERE, not on the in-memory sort
            // below: an id has no IComparable, so `.ThenBy(row => row.Id)` after
            // materialisation throws the moment two rows reach it. In SQL the
            // same expression translates and costs nothing.
            //
            // LINQ-to-Objects sorts stably, so ordering the source totally is
            // enough — the in-memory OrderBy/ThenBy below preserve this order
            // wherever their own keys tie.
            orderby registration.Id
            select new
            {
                registration.Id,
                MedicinalProductId = market.Id,
                market.GlobalProductId,
                ProductCode = product.Code,
                ProductName = product.Name,
                // The three facts the tier contributes to the answer. This is
                // the Registration context reading Product — the first read in
                // that direction, and deliberate: writes remain owned, reads
                // compose (ADR-039 principle 7).
                TradeNames = market.TradeNames
                    .OrderBy(name => name.Name)
                    .ThenBy(name => name.Id)
                    .Select(name => name.Name)
                    .ToList(),
                market.CurrentMarketStatus,
                Launches = market.MarketStatusHistory
                    .Where(entry =>
                        entry.Status == RegOS.Product.Domain.Product.MarketStatus.Launched)
                    .Select(entry => new
                    {
                        entry.OccurredOn,
                        entry.RecordedOnUtc,
                    })
                    .ToList(),
                MarketStatusOfRecord = market.Status,
                registration.AuthorityId,
                AuthorityName = authority.Name,
                HolderName = holder.LegalName,
                registration.RegistrationNumber,
                registration.CurrentStatus,
                registration.ApprovedOn,
                registration.ExpiresOn,
            }).ToListAsync(cancellationToken);

        var today = ExpiryVisibility.Today();

        // Deterministic: the source query is ordered by the registration id in
        // SQL and this sort is stable, so equal keys keep that order. The
        // tie-break lives there because an id has no IComparable in memory and
        // throws on the second row (BUG-001).
        return rows
            .OrderBy(row => Prominence(row.CurrentStatus))
            .ThenBy(row => row.ProductName.Value)
            // No id tie-break here — the source is already ordered by it and
            // this sort is stable. See the `orderby` in the query above.
            .Select(row =>
            {
                var expiry = ExpiryVisibility.For(
                    row.CurrentStatus, row.ExpiresOn, today);

                return new MarketRegistrationSummary(
                    row.Id.Value,
                    row.MedicinalProductId.Value,
                    row.GlobalProductId.Value,
                    row.ProductCode.Value,
                    row.ProductName.Value,
                    row.TradeNames,
                    row.CurrentMarketStatus.ToString(),
                    // Derived here exactly as it is on the market's own page:
                    // the first launch in business time, never stored.
                    row.Launches
                        // Deterministic: takes the earliest OccurredOn value,
                        // and entries sharing it are indistinguishable here.
                        .OrderBy(launch => launch.OccurredOn)
                        .ThenBy(launch => launch.RecordedOnUtc)
                        .Select(launch => (DateOnly?)launch.OccurredOn)
                        .FirstOrDefault(),
                    row.MarketStatusOfRecord
                        == RegOS.Product.Domain.Product.MedicinalProductStatus.Inactive,
                    row.AuthorityId.Value,
                    row.AuthorityName,
                    row.HolderName,
                    row.RegistrationNumber,
                    row.CurrentStatus.ToString(),
                    row.ApprovedOn,
                    row.ExpiresOn,
                    expiry.HasRunningValidity,
                    expiry.DaysUntilExpiry,
                    expiry.IsExpired);
            })
            .ToList();
    }

    /// <summary>
    /// Live authorisations first, then those still being decided, then those
    /// whose story has ended. A reading order, not a filter — nothing is hidden,
    /// and the enum's own ordinals are meaningless here.
    /// </summary>
    private static int Prominence(RegistrationStatus status) => status switch
    {
        RegistrationStatus.Approved => 0,
        RegistrationStatus.Suspended => 1,
        RegistrationStatus.UnderReview => 2,
        RegistrationStatus.Submitted => 3,
        RegistrationStatus.Planned => 4,
        RegistrationStatus.Expired => 5,
        RegistrationStatus.Withdrawn => 6,
        RegistrationStatus.Refused => 7,
        _ => 8,
    };
}
