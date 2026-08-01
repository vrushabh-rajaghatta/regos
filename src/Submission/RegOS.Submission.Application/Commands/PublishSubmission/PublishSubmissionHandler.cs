using RegOS.SharedKernel.Exceptions;
using RegOS.Submission.Application.Services;
using RegOS.Submission.Application.Validation;
using RegOS.Submission.Domain.Snapshot;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Commands.PublishSubmission;

/// <summary>
/// Publishes a submission once it is ready, under the next sequence number in its
/// application, and captures its immutable snapshot in the same operation. The
/// handler orchestrates but owns no business rules.
/// </summary>
/// <remarks>
/// Two rejection models meet here, deliberately (see ADR-009):
/// <list type="bullet">
///   <item><b>Lifecycle</b> — publishing something that is not a draft can never
///   succeed, so it raises <c>BusinessRuleViolationException</c> (409).</item>
///   <item><b>Readiness</b> — an incomplete dossier is a checklist the caller
///   works through, so every unmet criterion is returned together as a
///   validation result (400).</item>
/// </list>
/// Order matters: the lifecycle check runs first, so a published submission is
/// reported as a conflict rather than as a list of readiness issues.
/// <para>
/// <b>The sequence number is read here and arbitrated by the database.</b> The
/// number the policy returns is true when read and can be taken by a concurrent
/// publish before this one saves; the unique index on (application, sequence)
/// rejects the loser, which surfaces as a third 409 —
/// <see cref="SequenceNumberTakenException"/>. There is deliberately no
/// in-process retry; see the epic's S001 note for why the retry half of that
/// hypothesis did not survive contact with the change tracker.
/// </para>
/// </remarks>
public sealed class PublishSubmissionHandler
{
    private readonly SubmissionValidator _validator;
    private readonly ISubmissionNumberingPolicy _numbering;
    private readonly ISubmissionRepository _submissions;
    private readonly ISubmissionSnapshotRepository _snapshots;

    public PublishSubmissionHandler(
        SubmissionValidator validator,
        ISubmissionNumberingPolicy numbering,
        ISubmissionRepository submissions,
        ISubmissionSnapshotRepository snapshots)
    {
        _validator = validator;
        _numbering = numbering;
        _submissions = submissions;
        _snapshots = snapshots;
    }

    public async Task<PublishSubmissionResult> HandleAsync(
        PublishSubmissionCommand command,
        CancellationToken cancellationToken)
    {
        var submission = await _submissions.GetByIdAsync(
            command.SubmissionId, cancellationToken);

        if (submission is null)
        {
            throw new NotFoundException(
                $"Submission '{command.SubmissionId}' does not exist.");
        }

        // Lifecycle precondition first, and as an exception (409). Publishing a
        // submission that is no longer a draft is not an unmet readiness
        // criterion the caller can work through - it is a transition that can
        // never succeed. The aggregate owns the rule; asking it here rather
        // than duplicating the check keeps one source of truth.
        if (submission.Status != SubmissionStatus.Draft)
        {
            throw new BusinessRuleViolationException(
                SubmissionErrors.SubmissionNotDraft);
        }

        // Readiness second, and as a result (400). These are completeness
        // criteria for the dossier - a checklist the caller resolves and
        // retries - so every unmet one is reported at once rather than
        // surfacing a single exception.
        var validation = await _validator.ValidateAsync(
            command.SubmissionId, cancellationToken);

        if (!validation.IsValid)
        {
            return new PublishSubmissionResult(Published: false, validation);
        }

        // What this gets filed as. Read here rather than chosen by the aggregate:
        // a Submission is a root, so the other sequences in its application sit
        // outside its consistency boundary (ADR-044 decision 6).
        var next = await _numbering.GetNextPublishSequenceNumberAsync(
            submission.ApplicationId, cancellationToken);

        // The application layer supplies both facts — the number and the
        // timestamp — and the aggregate enforces the rules over them.
        submission.Publish(next.Number, next.PreviousPublished, DateTimeOffset.UtcNow);

        // Capture the published dossier. The application translates the submission
        // into the snapshot's input — the aggregate never sees a SubmissionSnapshot,
        // and the snapshot never sees a Submission.
        var snapshot = SubmissionSnapshot.Create(
            submission.TenantId,
            submission.Id,
            submission.Documents
                .OrderBy(d => d.DisplayOrder)
                .Select(d => (d.DocumentVersionId, d.DisplayOrder)));

        // Stage the snapshot, then commit both aggregates in one SaveChanges (one
        // transaction): if snapshot persistence fails, the publish rolls back too.
        await _snapshots.AddAsync(snapshot, cancellationToken);
        await _submissions.UpdateAsync(submission, cancellationToken);

        return new PublishSubmissionResult(Published: true, Validation: null);
    }
}
