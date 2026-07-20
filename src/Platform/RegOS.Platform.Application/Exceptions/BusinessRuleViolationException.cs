namespace RegOS.Platform.Application.Exceptions;

/// <summary>
/// Raised when an application-layer business rule is violated (e.g. the
/// organization is inactive, or the email is already in use). Distinct from the
/// domain's <see cref="SharedKernel.Exceptions.DomainException"/> so the API can
/// map it to HTTP 409 (conflict) while domain-invariant violations map to 400.
/// </summary>
public sealed class BusinessRuleViolationException : Exception
{
    public BusinessRuleViolationException(string message)
        : base(message)
    {
    }
}
