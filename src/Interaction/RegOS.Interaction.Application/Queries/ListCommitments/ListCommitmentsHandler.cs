using Microsoft.EntityFrameworkCore;

using RegOS.Interaction.Domain.Commitments;
using RegOS.Persistence;

namespace RegOS.Interaction.Application.Queries.ListCommitments;

public sealed class ListCommitmentsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListCommitmentsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CommitmentSummary>> HandleAsync(
        ListCommitmentsQuery query,
        CancellationToken cancellationToken)
    {
        var commitments = _dbContext.Commitments.AsNoTracking();

        if (!query.IncludeClosed)
        {
            commitments = commitments.Where(x =>
                x.CurrentStatus != CommitmentStatus.Fulfilled
                && x.CurrentStatus != CommitmentStatus.Waived);
        }

        return await commitments
            .OrderBy(x => x.DueOn)
            .Join(
                _dbContext.Authorities.AsNoTracking(),
                x => x.AuthorityId,
                a => a.Id,
                (x, a) => new CommitmentSummary(
                    x.Id.Value,
                    x.Title,
                    x.Description,
                    a.Id.Value,
                    a.Name,
                    // GivenOn and FulfilledOn are derived from the history and
                    // ignored by EF, so they are projected here rather than
                    // read from a column that does not exist.
                    x.History.OrderBy(h => h.OccurredOn).First().OccurredOn,
                    x.DueOn,
                    x.History
                        .Where(h => h.Status == CommitmentStatus.Fulfilled)
                        .Select(h => (DateOnly?)h.OccurredOn)
                        .FirstOrDefault(),
                    x.OwnerUserId != null ? x.OwnerUserId.Value : (Guid?)null,
                    x.SourceCorrespondenceId != null
                        ? x.SourceCorrespondenceId.Value
                        : (Guid?)null,
                    x.CurrentStatus.ToString(),
                    x.History
                        .OrderBy(h => h.OccurredOn)
                        .ThenBy(h => h.RecordedOnUtc)
                        .Select(h => new CommitmentHistoryEntry(
                            h.Status.ToString(),
                            h.OccurredOn,
                            h.RecordedOnUtc,
                            h.Note))
                        .ToList()))
            .ToListAsync(cancellationToken);
    }
}
