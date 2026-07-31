using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Application.Services;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Persistence;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Organization;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Organization.Infrastructure.Services;

public sealed class OrganizationSiteCreationPolicy
    : IOrganizationSiteCreationPolicy
{
    private readonly RegOSDbContext _dbContext;

    public OrganizationSiteCreationPolicy(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureCanCreateAsync(
        OrganizationId organizationId,
        CountryId countryId,
        IReadOnlyCollection<IdentifierSchemeId> schemeIds,
        CancellationToken cancellationToken)
    {
        // Read through the filtered set, so an organization belonging to
        // another tenant is "does not exist" rather than "forbidden" (ADR-031).
        var organization = await _dbContext.Organizations
            .AsNoTracking()
            .Where(x => x.Id == organizationId)
            .Select(x => new { x.Status })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(
                OrganizationSiteRuleErrors.OrganizationDoesNotExist);

        if (organization.Status != OrganizationStatus.Active)
            throw new BusinessRuleViolationException(
                OrganizationSiteRuleErrors.OrganizationInactive);

        var countryExists = await _dbContext.Countries
            .AsNoTracking()
            .AnyAsync(x => x.Id == countryId, cancellationToken);

        if (!countryExists)
            throw new DomainException(
                OrganizationSiteRuleErrors.CountryDoesNotExist);

        if (schemeIds.Count == 0)
            return;

        var distinct = schemeIds.Distinct().ToList();

        var known = await _dbContext.IdentifierSchemes
            .AsNoTracking()
            .CountAsync(x => distinct.Contains(x.Id), cancellationToken);

        if (known != distinct.Count)
            throw new DomainException(
                OrganizationSiteRuleErrors.IdentifierSchemeDoesNotExist);
    }
}
