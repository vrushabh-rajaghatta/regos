using RegOS.Submission.Application.Validation.Models;

namespace RegOS.Submission.Application.Commands.PublishSubmission;

/// <summary>
/// The outcome of a publish attempt. When <see cref="Published"/> is false the
/// submission was not ready and <see cref="Validation"/> carries the reasons, so the
/// caller can show them without issuing a second validation request. When true,
/// <see cref="Validation"/> is null.
/// </summary>
public sealed record PublishSubmissionResult(
    bool Published,
    SubmissionValidationResult? Validation);
