using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.ProductDocument.Domain.Entities;
using RegOS.Submission.Application.Validation.Models;
using RegOS.Submission.Domain.Submission;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Submission.Application.Validation;

/// <summary>
/// Answers "can this submission be published, and if not, why?" by inspecting a
/// submission and its evidence against the current business rules.
/// </summary>
/// <remarks>
/// Validation is a read-only query: it never modifies the database. Every reason a
/// submission is not ready to publish is reported as a <see cref="SubmissionValidationIssue"/>,
/// and the validator reports <em>all</em> of them rather than stopping at the first —
/// it never throws for a validation failure. The one exception is the addressed
/// resource itself: a submission that does not exist is a 404 concern, not a validation
/// issue, so it surfaces as a <see cref="NotFoundException"/> to stay consistent
/// with the rest of the Submission capability.
/// </remarks>
public sealed class SubmissionValidator
{
    private readonly ISubmissionRepository _submissions;
    private readonly RegOSDbContext _dbContext;

    public SubmissionValidator(
        ISubmissionRepository submissions,
        RegOSDbContext dbContext)
    {
        _submissions = submissions;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Validates whether the submission is ready to publish. Throws
    /// <see cref="NotFoundException"/> when the submission does not exist;
    /// otherwise returns a result carrying every issue found (possibly none).
    /// </summary>
    public async Task<SubmissionValidationResult> ValidateAsync(
        SubmissionId submissionId,
        CancellationToken cancellationToken)
    {
        var submission = await _submissions.GetByIdAsync(submissionId, cancellationToken);

        if (submission is null)
        {
            throw new NotFoundException(
                $"Submission '{submissionId}' does not exist.");
        }

        var result = new SubmissionValidationResult();

        // Rule: an already-published submission cannot be published again.
        if (submission.Status != SubmissionStatus.Draft)
        {
            result.AddIssue(
                SubmissionValidationCodes.SubmissionAlreadyPublished,
                "The submission has already been published and cannot be published again.");
        }

        // Rule: a submission must carry at least one document.
        if (submission.Documents.Count == 0)
        {
            result.AddIssue(
                SubmissionValidationCodes.SubmissionHasNoDocuments,
                "The submission has no documents attached.");
        }

        // Rule: every attached document version must still exist. This should never
        // fail under normal operation (the DocumentVersion FK is RESTRICT), but the
        // validator detects data corruption rather than trusting the invariant blindly.
        await AddMissingDocumentVersionIssuesAsync(submission, result, cancellationToken);

        return result;
    }

    private async Task AddMissingDocumentVersionIssuesAsync(
        SubmissionAggregate submission,
        SubmissionValidationResult result,
        CancellationToken cancellationToken)
    {
        var attachedVersionIds = submission.Documents
            .Select(d => d.DocumentVersionId)
            .ToList();

        if (attachedVersionIds.Count == 0)
        {
            return;
        }

        var existingVersionIds = await _dbContext.Set<DocumentVersion>()
            .Where(v => attachedVersionIds.Contains(v.Id))
            .Select(v => v.Id)
            .ToListAsync(cancellationToken);

        var missing = submission.Documents
            .Where(d => !existingVersionIds.Contains(d.DocumentVersionId));

        foreach (var document in missing)
        {
            result.AddIssue(
                SubmissionValidationCodes.MissingDocumentVersion,
                $"An attached document version no longer exists ({document.DocumentVersionId}).");
        }
    }
}
