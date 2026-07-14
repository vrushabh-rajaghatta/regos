namespace RegOS.Submission.Application.Exceptions;

/// <summary>
/// Raised when an application-layer business rule is violated (e.g. a
/// Submission Type from the wrong authority, or a closed application).
/// Mirrors the convention established in the RegulatoryApplication
/// capability; a future centralized exception filter can map it to 400.
/// </summary>
public sealed class BusinessRuleViolationException : Exception
{
    public BusinessRuleViolationException(string message)
        : base(message)
    {
    }
}
