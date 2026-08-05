using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Application.Services;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Infrastructure.Services;

public sealed class SubmissionPublicationBaseline : ISubmissionPublicationBaseline
{
    private readonly RegOSDbContext _dbContext;

    public SubmissionPublicationBaseline(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// Reads through the tenant query filter (ADR-031), which is correct rather
    /// than incidental: an application belongs to one tenant, so there is no
    /// cross-tenant sequence to miss.
    /// </remarks>
    public async Task<PublicationBaseline> GetAsync(
        RegulatoryApplicationId applicationId,
        CancellationToken cancellationToken)
    {
        // Only published submissions carry a sequence number (ADR-044), so "the
        // highest number here" and "the last thing we filed" are one question.
        var previous = await _dbContext.Submissions
            .AsNoTracking()
            .Where(x => x.ApplicationId == applicationId
                        && x.SequenceNumber != null)
            .OrderByDescending(x => x.SequenceNumber)
            .ThenBy(x => x.Id)
            .Select(x => new
            {
                SequenceNumber = x.SequenceNumber!.Value,
                Placements = x.Documents
                    .Where(d => d.TemplateSectionId != null)
                    .Select(d => new
                    {
                        d.Id,
                        d.ProductDocumentId,
                        TemplateSectionId = d.TemplateSectionId!.Value,
                        d.DocumentVersionId,
                    })
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (previous is null)
            return new PublicationBaseline(0, null, []);

        var placements = previous.Placements
            .Select(p => new PublishedPlacement(
                p.Id, p.ProductDocumentId, p.TemplateSectionId, p.DocumentVersionId))
            .ToList();

        return new PublicationBaseline(
            previous.SequenceNumber + 1, previous.SequenceNumber, placements);
    }
}
