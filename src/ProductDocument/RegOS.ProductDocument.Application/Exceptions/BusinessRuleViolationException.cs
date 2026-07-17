namespace RegOS.ProductDocument.Application.Exceptions;

/// <summary>
/// Raised when an application-layer business rule is violated (unknown/
/// inactive document type, empty file). Maps to 400.
/// </summary>
public sealed class BusinessRuleViolationException : Exception
{
    public BusinessRuleViolationException(string message)
        : base(message)
    {
    }
}
