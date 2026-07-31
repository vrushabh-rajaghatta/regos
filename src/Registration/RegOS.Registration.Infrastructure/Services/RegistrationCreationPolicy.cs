using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Registration.Application;
using RegOS.Registration.Application.Services;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Registration.Infrastructure.Services;

public sealed class RegistrationCreationPolicy : IRegistrationCreationPolicy
{
    private readonly RegOSDbContext _dbContext;

    public RegistrationCreationPolicy(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureCanCreateAsync(
        GlobalProductId globalProductId,
        CountryId countryId,
        AuthorityId authorityId,
        OrganizationId holderOrganizationId,
        RegulatoryApplicationId? originatingApplicationId,
        CancellationToken cancellationToken)
    {
        // Rule 1 — Product exists. The product is ADDRESSED by the route
        // (POST /api/products/{globalProductId}/registrations), so its absence is a
        // 404 like any other missing resource. The country, authority and
        // organization below are *referenced* values and stay 400.
        var productExists = await _dbContext.Products
            .AnyAsync(x => x.Id == globalProductId, cancellationToken);

        if (!productExists)
            throw new NotFoundException(
                RegistrationRuleErrors.ProductDoesNotExist);

        // Rule 2 — Country exists.
        var countryExists = await _dbContext.Countries
            .AnyAsync(x => x.Id == countryId, cancellationToken);

        if (!countryExists)
            throw new DomainException(
                RegistrationRuleErrors.CountryDoesNotExist);

        // Rule 3 — Authority exists (loaded once; reused for Rule 6).
        var authority = await _dbContext.Authorities
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == authorityId, cancellationToken);

        if (authority is null)
            throw new DomainException(
                RegistrationRuleErrors.AuthorityDoesNotExist);

        // Rule 4 — Holder organization exists (loaded because Rule 5 needs it).
        var holder = await _dbContext.Organizations
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == holderOrganizationId, cancellationToken);

        if (holder is null)
            throw new DomainException(
                RegistrationRuleErrors.OrganizationDoesNotExist);

        // Rule 5 — the holder must be active. An authorisation cannot be held
        // by an organization the tenant has retired.
        if (holder.Status != OrganizationStatus.Active)
            throw new BusinessRuleViolationException(
                RegistrationRuleErrors.OrganizationInactive);

        // Rule 6 — Authority belongs to the selected country.
        if (authority.CountryId != countryId)
            throw new DomainException(
                RegistrationRuleErrors.AuthorityNotInCountry);

        // Rule 7 — the originating application, when one is named, must exist
        // and belong to the same product. Naming another product's filing would
        // record a provenance that never happened.
        //
        // There is deliberately NO rule forbidding a second registration for the
        // same (product, country, authority): real portfolios hold several
        // authorisations in one market — different strengths, presentations, or
        // holders after a partial divestment. That is where this policy parts
        // company with the application one, which does forbid it.
        if (originatingApplicationId is not { } applicationId)
            return;

        var application = await _dbContext.RegulatoryApplications
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == applicationId, cancellationToken);

        if (application is null)
            throw new DomainException(
                RegistrationRuleErrors.ApplicationDoesNotExist);

        if (application.GlobalProductId != globalProductId)
            throw new DomainException(
                RegistrationRuleErrors.ApplicationNotForProduct);
    }
}
