using RegOS.Submission.Application.Validation;
using RegOS.Submission.Domain.Snapshot;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Commands.PublishSubmission;

/// <summary>
/// Publishes a submission once it is ready, and captures its immutable snapshot in
/// the same operation. The handler orchestrates but owns no business rules: readiness
/// is decided solely by the <see cref="SubmissionValidator"/>, the Draft-only
/// transition is enforced by the aggregate, and the snapshot copies published state.
/// A missing submission surfaces as the validator's <c>NotFoundException</c> (404).
/// </summary>
public sealed class PublishSubmissionHandler
{
    private readonly SubmissionValidator _validator;
    private readonly ISubmissionRepository _submissions;
    private readonly ISubmissionSnapshotRepository _snapshots;

    public PublishSubmissionHandler(
        SubmissionValidator validator,
        ISubmissionRepository submissions,
        ISubmissionSnapshotRepository snapshots)
    {
        _validator = validator;
        _submissions = submissions;
        _snapshots = snapshots;
    }

    public async Task<PublishSubmissionResult> HandleAsync(
        PublishSubmissionCommand command,
        CancellationToken cancellationToken)
    {
        // Single source of readiness. Throws NotFoundException when the
        // submission does not exist, so a missing resource stays a 404.
        var validation = await _validator.ValidateAsync(
            command.SubmissionId, cancellationToken);

        if (!validation.IsValid)
        {
            return new PublishSubmissionResult(Published: false, validation);
        }

        // Guaranteed non-null: the validator would have thrown otherwise.
        var submission = await _submissions.GetByIdAsync(
            command.SubmissionId, cancellationToken);

        // The application layer supplies the publication timestamp.
        submission!.Publish(DateTimeOffset.UtcNow);

        // Capture the published dossier. The application translates the submission
        // into the snapshot's input — the aggregate never sees a SubmissionSnapshot,
        // and the snapshot never sees a Submission.
        var snapshot = SubmissionSnapshot.Create(
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
