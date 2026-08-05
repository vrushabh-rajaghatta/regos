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
        // Materialised rather than projected, because the identifiers are an
        // owned collection and their scheme codes live in another context's
        // table. Same shape as GetOrganizationSite.
        var organization = await _dbContext.Organizations
            .AsNoTracking()
            .Include(x => x.Identifiers)
            .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException(OrganizationErrors.NotFound);

        var schemeIds = organization.Identifiers
            .Select(x => x.SchemeId)
            .ToList();

        var schemes = await _dbContext.IdentifierSchemes
            .AsNoTracking()
            .Where(x => schemeIds.Contains(x.Id))
            .ToDictionaryAsync(
                x => x.Id,
                x => new { x.Code, x.Name },
                cancellationToken);

        return new OrganizationDetails(
            organization.Id.Value,
            organization.LegalName,
            organization.Type,
            organization.Status,
            organization.StatusDate,
            organization.Acronym,
            organization.NameNativeLanguage,
            [.. organization.Identifiers
                .Select(identifier =>
                {
                    var scheme = schemes.GetValueOrDefault(identifier.SchemeId);

                    return new OrganizationIdentifierDto(
                        identifier.Id.Value,
                        identifier.SchemeId.Value,
                        scheme?.Code ?? string.Empty,
                        scheme?.Name ?? string.Empty,
                        identifier.Value);
                })
                // Deterministic: an organization carries one identifier per
                // scheme — the unique index on (OrganizationId, SchemeId).
                .OrderBy(identifier => identifier.SchemeCode)]);
    }
}
