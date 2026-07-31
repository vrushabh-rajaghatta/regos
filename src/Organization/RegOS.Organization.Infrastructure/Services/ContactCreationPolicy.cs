using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Application.Services;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.Persistence;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Organization;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Organization.Infrastructure.Services;

public sealed class ContactCreationPolicy : IContactCreationPolicy
{
    private readonly RegOSDbContext _dbContext;

    public ContactCreationPolicy(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureCanCreateAsync(
        OrganizationId organizationId,
        OrganizationSiteId? siteId,
        CountryId? countryId,
        IReadOnlyCollection<ContactRoleId> roleIds,
        CancellationToken cancellationToken)
    {
        var organization = await _dbContext.Organizations
            .AsNoTracking()
            .Where(x => x.Id == organizationId)
            .Select(x => new { x.Status })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(
                ContactRuleErrors.OrganizationDoesNotExist);

        if (organization.Status != OrganizationStatus.Active)
            throw new BusinessRuleViolationException(
                ContactRuleErrors.OrganizationInactive);

        if (siteId is not null)
        {
            var site = await _dbContext.OrganizationSites
                .AsNoTracking()
                .Where(x => x.Id == siteId)
                .Select(x => new { x.OrganizationId })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException(
                    ContactRuleErrors.SiteDoesNotExist);

            // A person cannot work at a site their employer does not operate.
            if (site.OrganizationId != organizationId)
                throw new DomainException(
                    ContactRuleErrors.SiteNotForOrganization);
        }

        if (countryId is { } country)
        {
            var exists = await _dbContext.Countries
                .AsNoTracking()
                .AnyAsync(x => x.Id == country, cancellationToken);

            if (!exists)
                throw new DomainException(
                    ContactRuleErrors.CountryDoesNotExist);
        }

        if (roleIds.Count == 0)
            return;

        var distinct = roleIds.Distinct().ToList();

        // Through the filtered set, so a role belonging to another tenant is
        // "does not exist" rather than usable.
        var known = await _dbContext.ContactRoles
            .AsNoTracking()
            .CountAsync(x => distinct.Contains(x.Id), cancellationToken);

        if (known != distinct.Count)
            throw new DomainException(ContactRuleErrors.RoleDoesNotExist);
    }
}
