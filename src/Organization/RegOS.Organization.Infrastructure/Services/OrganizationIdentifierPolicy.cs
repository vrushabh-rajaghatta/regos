using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Application.Services;
using RegOS.Persistence;
using RegOS.ReferenceData.Domain.Organization;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Organization.Infrastructure.Services;

public sealed class OrganizationIdentifierPolicy : IOrganizationIdentifierPolicy
{
    private readonly RegOSDbContext _dbContext;

    public OrganizationIdentifierPolicy(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureSchemeExistsAsync(
        IdentifierSchemeId schemeId,
        CancellationToken cancellationToken)
    {
        var exists = await _dbContext.IdentifierSchemes
            .AsNoTracking()
            .AnyAsync(x => x.Id == schemeId, cancellationToken);

        // Decidable from the request alone, so 400 rather than 404: the caller
        // named a registry that does not exist, they did not fail to find one.
        if (!exists)
            throw new DomainException(
                OrganizationIdentifierRuleErrors.SchemeDoesNotExist);
    }
}
