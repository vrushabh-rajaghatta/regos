using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.RegulatoryApplication.Application.Queries.GetRegulatoryApplication;

public sealed class GetRegulatoryApplicationHandler
{
    private readonly RegOSDbContext _dbContext;

    public GetRegulatoryApplicationHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RegulatoryApplicationDetail?> HandleAsync(
        ProductId productId,
        RegulatoryApplicationId applicationId,
        CancellationToken cancellationToken)
    {
        var row = await (
            from application in _dbContext.RegulatoryApplications.AsNoTracking()
            where application.ProductId == productId
                && application.Id == applicationId
            join country in _dbContext.Countries
                on application.CountryId equals country.Id
            join authority in _dbContext.Authorities
                on application.AuthorityId equals authority.Id
            join organization in _dbContext.Organizations
                on application.ApplicantOrganizationId equals organization.Id
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
            }).SingleOrDefaultAsync(cancellationToken);

        if (row is null)
            return null;

        return new RegulatoryApplicationDetail(
            row.Id.Value,
            row.Name,
            row.ApplicationNumber,
            row.Status.ToString(),
            row.CountryName,
            row.CountryCode,
            row.AuthorityName,
            row.AuthorityCode,
            row.OrganizationName);
    }
}
