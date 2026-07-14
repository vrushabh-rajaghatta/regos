using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.RegulatoryApplication.Application.Queries.Applications.GetApplication;

public sealed class GetApplicationHandler
{
    private readonly RegOSDbContext _dbContext;

    public GetApplicationHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApplicationDetailDto?> HandleAsync(
        RegulatoryApplicationId applicationId,
        CancellationToken cancellationToken)
    {
        // Single read-model projection joining the application to its
        // jurisdiction (country + authority) and applicant organization.
        // Status is materialized as the enum and stringified in memory —
        // it is mapped to an int column, so ToString() has no SQL translation.
        var row = await (
            from application in _dbContext.RegulatoryApplications.AsNoTracking()
            join country in _dbContext.Countries
                on application.CountryId equals country.Id
            join authority in _dbContext.Authorities
                on application.AuthorityId equals authority.Id
            join organization in _dbContext.Organizations
                on application.ApplicantOrganizationId equals organization.Id
            where application.Id == applicationId
            select new
            {
                application.Id,
                application.Name,
                application.ProductId,
                CountryId = country.Id,
                CountryName = country.Name,
                AuthorityId = authority.Id,
                AuthorityName = authority.Name,
                OrganizationId = organization.Id,
                OrganizationName = organization.LegalName,
                application.Status,
                application.CreatedOn,
            }).SingleOrDefaultAsync(cancellationToken);

        if (row is null)
            return null;

        return new ApplicationDetailDto(
            row.Id.Value,
            row.Name,
            row.ProductId.Value,
            row.CountryId.Value,
            row.CountryName,
            row.AuthorityId.Value,
            row.AuthorityName,
            row.OrganizationId.Value,
            row.OrganizationName,
            row.Status.ToString(),
            row.CreatedOn);
    }
}
