using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Domain.Snapshot;

/// <summary>
/// Persistence for published dossiers. There is deliberately no update method —
/// a snapshot is immutable once created, so the contract is create-and-read only.
/// </summary>
public interface ISubmissionSnapshotRepository
{
    Task AddAsync(
        SubmissionSnapshot snapshot,
        CancellationToken cancellationToken);

    /// <summary>Loads a snapshot with its documents, or null when it does not exist.</summary>
    Task<SubmissionSnapshot?> GetByIdAsync(
        SubmissionSnapshotId id,
        CancellationToken cancellationToken);

    /// <summary>
    /// Loads the snapshot for a submission, or null when the submission has not been
    /// published. There is at most one snapshot per submission.
    /// </summary>
    Task<SubmissionSnapshot?> GetBySubmissionIdAsync(
        SubmissionId submissionId,
        CancellationToken cancellationToken);
}
