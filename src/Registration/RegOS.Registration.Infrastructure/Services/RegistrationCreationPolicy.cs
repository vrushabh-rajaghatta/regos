using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Persistence;
using RegOS.Product.Domain.Product;
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
        MedicinalProductId medicinalProductId,
        AuthorityId authorityId,
        OrganizationId holderOrganizationId,
        RegulatoryApplicationId? originatingApplicationId,
        CancellationToken cancellationToken)
    {
        // Rule 1 — the medicinal product exists. It is ADDRESSED by the route
        // (POST /api/medicinal-products/{id}/registrations), so its absence is a
        // 404 like any other missing resource. The authority and organization
        // below are *referenced* values and stay 400.
        //
        // Loaded rather than probed with AnyAsync: rules 5 and 6 need the
        // country and the global product it localises, and those are the same
        // row. This is where the tier earns itself — the two facts a
        // registration used to carry are read from their one owner.
        var market = await _dbContext.MedicinalProducts
            .AsNoTracking()
            .Where(x => x.Id == medicinalProductId)
            .Select(x => new { x.CountryId, x.GlobalProductId })
            .SingleOrDefaultAsync(cancellationToken);

        if (market is null)
            throw new NotFoundException(
                RegistrationRuleErrors.MedicinalProductDoesNotExist);

        // Rule 2 — Authority exists (loaded once; reused for Rule 5).
        var authority = await _dbContext.Authorities
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == authorityId, cancellationToken);

        if (authority is null)
            throw new DomainException(
                RegistrationRuleErrors.AuthorityDoesNotExist);

        // Rule 3 — Holder organization exists (loaded because Rule 4 needs it).
        var holder = await _dbContext.Organizations
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == holderOrganizationId, cancellationToken);

        if (holder is null)
            throw new DomainException(
                RegistrationRuleErrors.OrganizationDoesNotExist);

        // Rule 4 — the holder must be active. An authorisation cannot be held
        // by an organization the tenant has retired.
        if (holder.Status != OrganizationStatus.Active)
            throw new BusinessRuleViolationException(
                RegistrationRuleErrors.OrganizationInactive);

        // Rule 5 — the authority must regulate the market the medicinal product
        // is in. The country is read from the tier, not from the caller: there
        // is no longer a way to state one that disagrees with it.
        if (authority.CountryId != market.CountryId)
            throw new DomainException(
                RegistrationRuleErrors.AuthorityNotInCountry);

        // Rule 6 — the originating application, when one is named, must exist
        // and belong to the same global product. Naming another product's
        // filing would record a provenance that never happened. Applications
        // stay at the global tier (Phase 2 decision 2), so the comparison
        // reaches up through the medicinal product to make it.
        //
        // There is deliberately NO rule forbidding a second registration for
        // the same medicinal product: real portfolios hold several
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

        if (application.GlobalProductId != market.GlobalProductId)
            throw new DomainException(
                RegistrationRuleErrors.ApplicationNotForProduct);
    }
}
