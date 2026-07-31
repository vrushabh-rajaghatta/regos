using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;

namespace RegOS.RegulatoryApplication.Application.Queries.ListRegulatoryApplications;

public sealed class ListRegulatoryApplicationsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListRegulatoryApplicationsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ListRegulatoryApplicationsResult> HandleAsync(
        GlobalProductId globalProductId,
        CancellationToken cancellationToken)
    {
        // Read model joins the application to its jurisdiction (country +
        // authority) and applicant organization. The aggregate stores only
        // the ids; the names live in their owning capabilities.
        var rows = await (
            from application in _dbContext.RegulatoryApplications.AsNoTracking()
            where application.GlobalProductId == globalProductId
            join country in _dbContext.Countries
                on application.CountryId equals country.Id
            join authority in _dbContext.Authorities
                on application.AuthorityId equals authority.Id
            join organization in _dbContext.Organizations
                on application.ApplicantOrganizationId equals organization.Id
            orderby application.Name
            select new
            {
                application.Id,
                application.Name,
                application.ApplicationNumber,
                application.Status,
                CountryName = country.Name,
                CountryCode = country.Code,
                AuthorityName = authority.Name,
                AuthorityCode = authority.Code,
                OrganizationName = organization.LegalName,
            }).ToListAsync(cancellationToken);

        var applications = rows
            .Select(row => new RegulatoryApplicationInfo(
                row.Id.Value,
                row.Name,
                row.ApplicationNumber,
                row.Status.ToString(),
                row.CountryName,
                row.CountryCode,
                row.AuthorityName,
                row.AuthorityCode,
                row.OrganizationName))
            .ToList();

        return new ListRegulatoryApplicationsResult(applications);
    }
}
