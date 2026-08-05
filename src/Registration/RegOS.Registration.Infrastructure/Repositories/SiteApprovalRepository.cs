using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Registration.Domain.Aggregates.SiteApprovals;

namespace RegOS.Registration.Infrastructure.Repositories;

public sealed class SiteApprovalRepository : ISiteApprovalRepository
{
    private readonly RegOSDbContext _dbContext;

    public SiteApprovalRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        SiteApproval approval,
        CancellationToken cancellationToken)
    {
        _dbContext.SiteApprovals.Add(approval);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <remarks>
    /// No <c>Include</c>: the aggregate holds two ids and a date, and reasons
    /// across nothing — the shape a relationship should have.
    /// </remarks>
    public async Task<SiteApproval?> GetByIdAsync(
        SiteApprovalId id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.SiteApprovals
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(
        SiteApproval approval,
        CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <remarks>
    /// <b>Removed, not deactivated</b>, and this is the same exception ES-018
    /// leaves room for that <c>PackAuthorisationRepository</c> takes: an
    /// approval recorded against the wrong licence is a mistake about a
    /// relationship, not an event that happened. A site genuinely *removed*
    /// from a licence by variation is a different act, and when somebody asks
    /// for it, it is a date on this row rather than a delete.
    /// </remarks>
    public async Task RemoveAsync(
        SiteApproval approval,
        CancellationToken cancellationToken)
    {
        _dbContext.SiteApprovals.Remove(approval);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
