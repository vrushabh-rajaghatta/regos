using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.RegulatoryApplication.Application.Services;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Exceptions;

using RegulatoryApplicationAggregate =
    RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;

namespace RegOS.RegulatoryApplication.Infrastructure.Services;

/// <inheritdoc />
public sealed class ApplicationNumberPolicy : IApplicationNumberPolicy
{
    private readonly RegOSDbContext _dbContext;

    public ApplicationNumberPolicy(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureTheNumberCanStillChangeAsync(
        RegulatoryApplicationAggregate application,
        string proposedNumber,
        CancellationToken cancellationToken)
    {
        // Recording the same value again changes nothing, so nothing is at
        // stake. Worth allowing rather than refusing: an idempotent write is
        // not a correction.
        if (string.Equals(
                application.ApplicationNumber,
                proposedNumber.Trim(),
                StringComparison.Ordinal))
        {
            return;
        }

        // Only a published sequence has been filed. A draft carries nothing to
        // the authority, so a number can still be corrected around one — which
        // is the ordinary case while an application is being prepared.
        var filed = await _dbContext.Submissions
            .AsNoTracking()
            .Where(x => x.ApplicationId == application.Id
                && x.SequenceNumber != null)
            .OrderBy(x => x.SequenceNumber)
            .Select(x => x.SequenceNumber)
            .FirstOrDefaultAsync(cancellationToken);

        // Null only when no sequence has been published — the number was never
        // transmitted, so nothing downstream depends on it.
        if (filed is null || application.ApplicationNumber is null)
            return;

        throw new BusinessRuleViolationException(string.Format(
            ApplicationErrors.ApplicationNumberIsFiled,
            $"{filed:0000}",
            application.ApplicationNumber));
    }
}
