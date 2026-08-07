using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.Registration.Application.Queries.ListSiteAlignment;

public sealed class ListSiteAlignmentHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListSiteAlignmentHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// <b>Two reads, joined in memory, and that is the point rather than a
    /// compromise.</b> A site may manufacture without being approved, be
    /// approved without manufacturing, or both — which is a full outer join, and
    /// writing one in EF would be harder to read than the union below without
    /// being faster on a list this size.
    /// <para>
    /// <b>Both start at filtered roots</b> (ADR-031). Manufacturing operations
    /// carry a <c>TenantId</c>; approvals are reached through this market's
    /// registrations, which carry one too.
    /// </para>
    /// <para>
    /// <b>The comparison is derived here and stored nowhere.</b> A persisted
    /// "aligned" flag would rot the moment either side moved, and both sides
    /// move: a transfer opens a new operation, a variation adds a site to a
    /// licence. The EPIC-005 expiry precedent, used a fourth time.
    /// </para>
    /// </remarks>
    public async Task<IReadOnlyList<SiteAlignment>> HandleAsync(
        ListSiteAlignmentQuery query,
        CancellationToken cancellationToken)
    {
        // What we do. Current periods only — a site that stopped in 2023 is
        // history, and an advisory about it would make every transfer look
        // like a finding.
        var operations = await _dbContext.ManufacturingOperations
            .AsNoTracking()
            .Where(x => x.MedicinalProductId == query.MedicinalProductId
                && x.CeasedOn == null)
            .Select(x => new
            {
                x.OrganizationSiteId,
                x.Operation.Display,
            })
            .ToListAsync(cancellationToken);

        // What the licences permit.
        var approvals = await (
            from approval in _dbContext.SiteApprovals.AsNoTracking()
            join registration in _dbContext.Registrations
                on approval.RegistrationId equals registration.Id
            where registration.MedicinalProductId == query.MedicinalProductId
            select new
            {
                approval.OrganizationSiteId,
                registration.RegistrationNumber,
                approval.ApprovedOn,
            })
            .ToListAsync(cancellationToken);

        var siteIds = operations
            .Select(x => x.OrganizationSiteId)
            .Concat(approvals.Select(x => x.OrganizationSiteId))
            .Distinct()
            .ToList();

        if (siteIds.Count == 0)
            return [];

        var sites = await (
            from site in _dbContext.OrganizationSites.AsNoTracking()
            where siteIds.Contains(site.Id)
            join country in _dbContext.Countries
                on site.Address.CountryId equals country.Id
            // BUG-001. The tie-breaker belongs here, in SQL, where an id has a
            // translation — after materialisation it has no IComparable and
            // throws the moment a product has two sites.
            orderby site.Id
            select new
            {
                site.Id,
                site.Name,
                CountryName = country.Name,
            })
            .ToListAsync(cancellationToken);

        // Deterministic: the source query is ordered by site id in SQL and this
        // sort is stable, so equal names keep that order. The tie-break lives
        // there because an id has no IComparable in memory (BUG-001).
        return sites
            .OrderBy(site => site.Name, StringComparer.Ordinal)
            .Select(site =>
            {
                var performed = operations
                    .Where(x => x.OrganizationSiteId == site.Id)
                    .Select(x => x.Display)
                    // Deterministic: sorting strings by their own value —
                    // equal ones are indistinguishable in the output.
                    .OrderBy(x => x, StringComparer.Ordinal)
                    .ToList();

                var named = approvals
                    .Where(x => x.OrganizationSiteId == site.Id)
                    // Deterministic: an approval is unique per
                    // (RegistrationId, OrganizationSiteId), and this list is
                    // already narrowed to one site.
                    .OrderBy(x => x.ApprovedOn)
                    .Select(x => new SiteAlignmentApproval(
                        x.RegistrationNumber, x.ApprovedOn))
                    .ToList();

                return new SiteAlignment(
                    site.Id.Value,
                    site.Name,
                    site.CountryName,
                    performed,
                    named,
                    performed.Count > 0,
                    named.Count > 0);
            })
            .ToList();
    }
}
