namespace RegOS.RegulatoryApplication.Application.Exceptions;

/// <summary>
/// Raised when an application-layer business rule is violated. Kept distinct from
/// generic framework exceptions so that centralized exception mapping (a later
/// infrastructure improvement) can translate it to HTTP 409/422 rather than 500.
/// </summary>
public sealed class BusinessRuleViolationException : Exception
{
    public BusinessRuleViolationException(string message)
        : base(message)
    {
    }
}
