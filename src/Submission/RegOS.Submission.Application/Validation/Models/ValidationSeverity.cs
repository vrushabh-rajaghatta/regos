namespace RegOS.Submission.Application.Validation.Models;

/// <summary>
/// How strongly a validation issue affects a submission's readiness to publish.
/// Only <see cref="Error"/> is produced today; <see cref="Warning"/> and
/// <see cref="Information"/> exist so advisory rules can be added later without
/// reshaping the result model or its consumers.
/// </summary>
public enum ValidationSeverity
{
    Information = 1,
    Warning = 2,
    Error = 3,
}
