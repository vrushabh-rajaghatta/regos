using Microsoft.EntityFrameworkCore;

using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.RegulatoryApplication.Application;
using RegOS.RegulatoryApplication.Application.Exceptions;
using RegOS.RegulatoryApplication.Application.Services;

namespace RegOS.RegulatoryApplication.Infrastructure.Services;

public sealed class RegulatoryApplicationCreationPolicy
    : IRegulatoryApplicationCreationPolicy
{
    private readonly RegOSDbContext _dbContext;

    public RegulatoryApplicationCreationPolicy(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureCanCreateAsync(
        ProductId productId,
        CountryId countryId,
        AuthorityId authorityId,
        OrganizationId organizationId,
        CancellationToken cancellationToken)
    {
        // Rule 1 — Product exists.
        var productExists = await _dbContext.Products
            .AnyAsync(x => x.Id == productId, cancellationToken);

        if (!productExists)
            throw new BusinessRuleViolationException(
                RegulatoryApplicationErrors.ProductDoesNotExist);

        // Rule 2 — Country exists.
        var countryExists = await _dbContext.Countries
            .AnyAsync(x => x.Id == countryId, cancellationToken);

        if (!countryExists)
            throw new BusinessRuleViolationException(
                RegulatoryApplicationErrors.CountryDoesNotExist);

        // Rule 3 — Authority exists (loaded once; reused for Rule 6).
        var authority = await _dbContext.Authorities
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == authorityId, cancellationToken);

        if (authority is null)
            throw new BusinessRuleViolationException(
                RegulatoryApplicationErrors.AuthorityDoesNotExist);

        // Rule 4 — Organization exists (loaded because Rule 5 needs its status).
        var organization = await _dbContext.Organizations
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == organizationId, cancellationToken);

        if (organization is null)
            throw new BusinessRuleViolationException(
                RegulatoryApplicationErrors.OrganizationDoesNotExist);

        // Rule 5 — Organization must be active.
        if (organization.Status != OrganizationStatus.Active)
            throw new BusinessRuleViolationException(
                RegulatoryApplicationErrors.OrganizationInactive);

        // Rule 6 — Authority belongs to the selected country.
        if (authority.CountryId != countryId)
            throw new BusinessRuleViolationException(
                RegulatoryApplicationErrors.AuthorityNotInCountry);

        // Rule 7 — No duplicate application for the same jurisdiction.
        var duplicateExists = await _dbContext.RegulatoryApplications
            .AnyAsync(
                x => x.ProductId == productId
                    && x.CountryId == countryId
                    && x.AuthorityId == authorityId,
                cancellationToken);

        if (duplicateExists)
            throw new BusinessRuleViolationException(
                RegulatoryApplicationErrors.DuplicateApplication);
    }
}
