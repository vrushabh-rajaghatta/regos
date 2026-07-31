using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;

using RegistrationAggregate = RegOS.Registration.Domain.Aggregates.Registration.Registration;

namespace RegOS.Registration.Application.Queries.ListProductRegistrations;

/// <summary>
/// "Where is this product registered?" — one half of the portfolio question.
/// The mirror, "what do we hold in this market?", is
/// <c>ListMarketRegistrationsHandler</c>.
/// </summary>
public sealed class ListProductRegistrationsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListProductRegistrationsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns the product's registrations, or null when the product does not
    /// exist — so the endpoint can 404 rather than return an empty list for a
    /// product that was never there.
    /// </summary>
    public async Task<IReadOnlyList<RegistrationSummary>?> HandleAsync(
        GlobalProductId globalProductId,
        CancellationToken cancellationToken)
    {
        var productExists = await _dbContext.Products
            .AsNoTracking()
            .AnyAsync(x => x.Id == globalProductId, cancellationToken);

        if (!productExists)
            return null;

        // Strongly-typed ids are materialised then unwrapped in memory: their
        // converters have no SQL translation for .Value.
        var rows = await (
            from registration in _dbContext.Set<RegistrationAggregate>()
                .AsNoTracking()
            where registration.GlobalProductId == globalProductId
            join country in _dbContext.Countries
                on registration.CountryId equals country.Id
            join authority in _dbContext.Authorities
                on registration.AuthorityId equals authority.Id
            join holder in _dbContext.Organizations
                on registration.HolderOrganizationId equals holder.Id
            orderby country.Name, registration.CreatedOn
            select new
            {
                registration.Id,
                registration.CountryId,
                CountryName = country.Name,
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
            .Select(row =>
            {
                var expiry = ExpiryVisibility.For(
                    row.CurrentStatus, row.ExpiresOn, today);

                return new RegistrationSummary(
                    row.Id.Value,
                    row.CountryId.Value,
                    row.CountryName,
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
}
