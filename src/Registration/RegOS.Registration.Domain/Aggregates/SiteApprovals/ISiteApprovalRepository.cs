namespace RegOS.Registration.Domain.Aggregates.SiteApprovals;

public interface ISiteApprovalRepository
{
    Task AddAsync(SiteApproval approval, CancellationToken cancellationToken);

    /// <summary>Tracked — for mutation.</summary>
    Task<SiteApproval?> GetByIdAsync(
        SiteApprovalId id,
        CancellationToken cancellationToken);

    Task UpdateAsync(SiteApproval approval, CancellationToken cancellationToken);

    Task RemoveAsync(SiteApproval approval, CancellationToken cancellationToken);
}
