using RegOS.Submission.Application.Validation;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Commands.PublishSubmission;

/// <summary>
/// Publishes a submission once it is ready. The handler orchestrates but owns no
/// business rules: readiness is decided solely by the <see cref="SubmissionValidator"/>,
/// and the Draft-only transition is enforced by the aggregate. A missing submission
/// surfaces as the validator's <c>SubmissionNotFoundException</c> (404).
/// </summary>
public sealed class PublishSubmissionHandler
{
    private readonly SubmissionValidator _validator;
    private readonly ISubmissionRepository _submissions;

    public PublishSubmissionHandler(
        SubmissionValidator validator,
        ISubmissionRepository submissions)
    {
        _validator = validator;
        _submissions = submissions;
    }

    public async Task<PublishSubmissionResult> HandleAsync(
        PublishSubmissionCommand command,
        CancellationToken cancellationToken)
    {
        // Single source of readiness. Throws SubmissionNotFoundException when the
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

        submission!.Publish();
        await _submissions.UpdateAsync(submission, cancellationToken);

        return new PublishSubmissionResult(Published: true, Validation: null);
    }
}
