namespace RegOS.Submission.Domain.Submission;

public interface ISubmissionRepository
{
    Task AddAsync(
        Submission submission,
        CancellationToken cancellationToken);

    /// <summary>
    /// Loads a tracked Submission with its document collection, ready for
    /// mutation. Returns null when the submission does not exist.
    /// </summary>
    Task<Submission?> GetByIdAsync(
        SubmissionId id,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        Submission submission,
        CancellationToken cancellationToken);
}
