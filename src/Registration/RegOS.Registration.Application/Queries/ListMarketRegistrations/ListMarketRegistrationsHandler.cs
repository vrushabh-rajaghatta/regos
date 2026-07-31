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
            select new
            {
                registration.Id,
                market.GlobalProductId,
                ProductCode = product.Code,
                ProductName = product.Name,
                registration.AuthorityId,
                AuthorityName = authority.Name,
                HolderName = holder.LegalName,
                registration.RegistrationNumber,
                registration.CurrentStatus,
                registration.ApprovedOn,
                registration.ExpiresOn,
            }).ToListAsync(cancellationToken);

        var today = ExpiryVisibility.Today();

        return rows
            .OrderBy(row => Prominence(row.CurrentStatus))
            .ThenBy(row => row.ProductName.Value)
            .Select(row =>
            {
                var expiry = ExpiryVisibility.For(
                    row.CurrentStatus, row.ExpiresOn, today);

                return new MarketRegistrationSummary(
                    row.Id.Value,
                    row.GlobalProductId.Value,
                    row.ProductCode.Value,
                    row.ProductName.Value,
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
