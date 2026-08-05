using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.Registration.Application.Queries.ListApprovedSites;

public sealed class ListApprovedSitesHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListApprovedSitesHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// <b>Starts at <c>SiteApprovals</c>, which is a filtered root</b>
    /// (ADR-031), and reaches the licence and the site from it. Both carry a
    /// <c>TenantId</c> of their own, so the joins cannot widen what the first
    /// filter allowed.
    /// <para>
    /// <b>Grouped by site rather than by licence</b>, because the question the
    /// next story asks is about a site — <em>"is the plant we manufacture at
    /// approved here?"</em> — and a site named on two of this market's licences
    /// should read as one approved site, not two rows.
    /// </para>
    /// </remarks>
    public async Task<IReadOnlyList<ApprovedSiteSummary>> HandleAsync(
        ListApprovedSitesQuery query,
        CancellationToken cancellationToken)
    {
        var rows = await (
            from approval in _dbContext.SiteApprovals.AsNoTracking()
            join registration in _dbContext.Registrations
                on approval.RegistrationId equals registration.Id
            where registration.MedicinalProductId == query.MedicinalProductId
            join site in _dbContext.OrganizationSites
                on approval.OrganizationSiteId equals site.Id
            join country in _dbContext.Countries
                on site.Address.CountryId equals country.Id
            orderby site.Name, approval.ApprovedOn, approval.Id
            select new
            {
                ApprovalId = approval.Id,
                SiteId = site.Id,
                SiteName = site.Name,
                CountryName = country.Name,
                approval.RegistrationId,
                registration.RegistrationNumber,
                registration.CurrentStatus,
                approval.ApprovedOn,
            })
            .ToListAsync(cancellationToken);

        // Strongly-typed ids are materialised then unwrapped in memory: their
        // converters have no SQL translation for .Value.
        return rows
            .GroupBy(row => new { row.SiteId, row.SiteName, row.CountryName })
            .Select(group => new ApprovedSiteSummary(
                group.Key.SiteId.Value,
                group.Key.SiteName,
                group.Key.CountryName,
                [.. group.Select(row => new SiteApprovalSummary(
                    row.ApprovalId.Value,
                    row.RegistrationId.Value,
                    row.RegistrationNumber,
                    row.CurrentStatus.ToString(),
                    row.ApprovedOn))]))
            .ToList();
    }
}
