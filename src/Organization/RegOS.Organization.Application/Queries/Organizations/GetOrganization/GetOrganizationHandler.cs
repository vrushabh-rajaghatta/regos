using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Persistence;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Organization.Application.Queries.Organizations.GetOrganization;

/// <summary>
/// Reads a single organization straight from the database: no repository, no
/// aggregate, no tracking (ADR-016).
///
/// No manual tenant clause — the global query filter scopes this to the
/// caller's registry (ADR-032). Another tenant's organization is
/// indistinguishable from one that does not exist. (This handler once
/// documented the opposite: under the fused model an organization *was* a
/// tenant, and scoping the read would have reduced the directory to one row.)
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
