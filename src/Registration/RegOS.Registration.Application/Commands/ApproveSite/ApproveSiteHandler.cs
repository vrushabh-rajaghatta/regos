using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Registration.Domain.Aggregates.SiteApprovals;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Registration.Application.Commands.ApproveSite;

public sealed class ApproveSiteHandler
{
    private readonly ISiteApprovalRepository _approvals;
    private readonly RegOSDbContext _dbContext;
    private readonly ITenantContext _tenant;

    public ApproveSiteHandler(
        ISiteApprovalRepository approvals,
        RegOSDbContext dbContext,
        ITenantContext tenant)
    {
        _approvals = approvals;
        _dbContext = dbContext;
        _tenant = tenant;
    }

    /// <remarks>
    /// <b>No market agreement is checked, and that is the difference from
    /// <c>AuthorisePack</c>.</b> A pack authorised by a licence from another
    /// market is a data error — both name a medicinal product, and they must
    /// agree. A <em>site</em> has no market: one plant in Germany supplies
    /// licences in eight countries, which is exactly the case this epic exists
    /// to reason about. There is nothing here to disagree.
    /// </remarks>
    public async Task<ApproveSiteResult> HandleAsync(
        ApproveSiteCommand command,
        CancellationToken cancellationToken)
    {
        // Both reads go through the fail-closed filters, so another tenant's
        // licence or site is not found rather than refused (ADR-031).
        var licenceExists = await _dbContext.Registrations
            .AsNoTracking()
            .AnyAsync(x => x.Id == command.RegistrationId, cancellationToken);

        if (!licenceExists)
            throw new NotFoundException(
                SiteApprovalErrors.RegistrationDoesNotExist);

        var siteExists = await _dbContext.OrganizationSites
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == command.OrganizationSiteId, cancellationToken);

        if (!siteExists)
            throw new NotFoundException(SiteApprovalErrors.SiteDoesNotExist);

        var alreadySaid = await _dbContext.SiteApprovals
            .AsNoTracking()
            .AnyAsync(
                x => x.RegistrationId == command.RegistrationId
                    && x.OrganizationSiteId == command.OrganizationSiteId,
                cancellationToken);

        // The unique index says the same thing where a race cannot slip past
        // this check; here so the refusal names the act rather than a constraint.
        if (alreadySaid)
            throw new BusinessRuleViolationException(
                SiteApprovalErrors.AlreadyApproved);

        var approval = SiteApproval.Create(
            _tenant.TenantId,
            command.RegistrationId,
            command.OrganizationSiteId,
            command.ApprovedOn);

        await _approvals.AddAsync(approval, cancellationToken);

        return new ApproveSiteResult(approval.Id.Value);
    }
}
