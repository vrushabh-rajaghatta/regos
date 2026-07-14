namespace RegOS.Submission.Application.Exceptions;

/// <summary>
/// Raised when the parent Application named in the route does not exist.
/// The Application is the addressed resource, so this maps to 404 rather
/// than a 400 business-rule violation.
/// </summary>
public sealed class ApplicationNotFoundException : Exception
{
    public ApplicationNotFoundException(string message)
        : base(message)
    {
    }
}
