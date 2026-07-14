namespace RegOS.Submission.Domain.Submission;

public interface ISubmissionRepository
{
    Task AddAsync(
        Submission submission,
        CancellationToken cancellationToken);
}
