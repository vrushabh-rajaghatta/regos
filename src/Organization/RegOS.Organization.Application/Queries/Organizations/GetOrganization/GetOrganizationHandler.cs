using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Persistence;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Organization.Application.Queries.Organizations.GetOrganization;

/// <summary>
/// Reads a single organization straight from the database: no repository, no
/// aggregate, no tracking (ADR-016).
///
/// No tenant filter, unlike the product and user detail queries. An
/// organization *is* a tenant, so scoping this read to the caller's own
/// organization would reduce the directory to a single row.
/// </summary>
public sealed class GetOrganizationHandler
{
    private readonly RegOSDbContext _dbContext;

    public GetOrganizationHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrganizationDetails> HandleAsync(
        GetOrganizationQuery query,
        CancellationToken cancellationToken)
    {
        var organization = await _dbContext.Organizations
            .AsNoTracking()
            .Where(x => x.Id == query.Id)
            .Select(x => new OrganizationDetails(
                x.Id.Value,
                x.LegalName,
                x.Type,
                x.Status))
            .SingleOrDefaultAsync(cancellationToken);

        return organization
            ?? throw new NotFoundException(OrganizationErrors.NotFound);
    }
}
