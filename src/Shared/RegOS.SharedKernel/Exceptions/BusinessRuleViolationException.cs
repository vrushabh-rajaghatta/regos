namespace RegOS.SharedKernel.Exceptions;

/// <summary>
/// The request is valid, but the current business state forbids it. Mapped to
/// HTTP 409. Examples: inviting a user whose email is already taken, adding a
/// user to an inactive organization, removing a document from a submission that
/// is no longer a draft.
/// </summary>
/// <remarks>
/// Contrast with the base <see cref="DomainException"/> (400), where the
/// <em>payload</em> is what is wrong. Here the payload is fine and would
/// succeed against a different system state, which is precisely what 409
/// expresses. Derives from <see cref="DomainException"/> because it is still a
/// business failure; it simply maps differently.
/// </remarks>
public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string message)
        : base(message)
    {
    }
}
