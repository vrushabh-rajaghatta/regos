namespace RegOS.Submission.Application.Exceptions;

/// <summary>
/// Raised when the Submission named in the route does not exist. The
/// Submission is the addressed resource for document attach/remove, so this
/// maps to 404.
/// </summary>
public sealed class SubmissionNotFoundException : Exception
{
    public SubmissionNotFoundException(string message)
        : base(message)
    {
    }
}
