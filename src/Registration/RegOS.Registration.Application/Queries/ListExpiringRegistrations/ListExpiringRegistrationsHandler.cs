using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

using RegistrationAggregate = RegOS.Registration.Domain.Aggregates.Registration.Registration;

namespace RegOS.Registration.Application.Queries.ListExpiringRegistrations;

/// <summary>
/// "Which registrations deserve attention today?" — the third portfolio
/// question, and the only one that spans every market at once.
/// </summary>
/// <remarks>
/// The set is objective: <b>registrations whose validity is still running</b>,
/// nearest expiry first. Not "expiring within ninety days" — that is a
/// threshold, and thresholds are policy that changes by market, by tenant and
/// by year. Ordering makes the list immediately useful; deciding what counts as
/// urgent is left to whoever is reading it.
/// <para>
/// Deliberately unlimited. A silent "top ten" would hide the eleventh and read
/// as completeness. If a portfolio ever outgrows one response that is
/// pagination, which fits on top of this shape rather than replacing it.
/// </para>
/// </remarks>
public sealed class ListExpiringRegistrationsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListExpiringRegistrationsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ExpiringRegistration>> HandleAsync(
        CancellationToken cancellationToken)
    {
        var rows = await (
            from registration in _dbContext.Set<RegistrationAggregate>()
                .AsNoTracking()
            where registration.ExpiresOn != null
            join market in _dbContext.MedicinalProducts
                on registration.MedicinalProductId equals market.Id
            join product in _dbContext.Products
                on market.GlobalProductId equals product.Id
            join country in _dbContext.Countries
                on market.CountryId equals country.Id
            select new
            {
                registration.Id,
                market.GlobalProductId,
                ProductName = product.Name,
                market.CountryId,
                CountryName = country.Name,
                registration.RegistrationNumber,
                registration.CurrentStatus,
                registration.ExpiresOn,
            }).ToListAsync(cancellationToken);

        var today = ExpiryVisibility.Today();

        // Whether a countdown is still running is decided by the same lifecycle
        // policy every transition answers to, so it is applied here rather than
        // duplicated as a list of statuses in the SQL above.
        return rows
            .Select(row => new
            {
                Row = row,
                Expiry = ExpiryVisibility.For(
                    row.CurrentStatus, row.ExpiresOn, today),
            })
            .Where(x => x.Expiry.DaysUntilExpiry is not null)
            .OrderBy(x => x.Expiry.DaysUntilExpiry)
            .ThenBy(x => x.Row.ProductName.Value)
            .Select(x => new ExpiringRegistration(
                x.Row.Id.Value,
                x.Row.GlobalProductId.Value,
                x.Row.ProductName.Value,
                x.Row.CountryId.Value,
                x.Row.CountryName,
                x.Row.RegistrationNumber,
                x.Row.CurrentStatus.ToString(),
                x.Row.ExpiresOn!.Value,
                x.Expiry.DaysUntilExpiry!.Value,
                x.Expiry.IsExpired))
            .ToList();
    }
}
