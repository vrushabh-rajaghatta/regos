namespace RegOS.SharedKernel.Exceptions;

/// <summary>
/// The base of RegOS's shared failure model, and on its own the "the request
/// itself is invalid" case — mapped to HTTP 400. Signals that a business rule
/// was broken, not a technical or programming fault, which is why the domain
/// raises it in preference to <see cref="ArgumentException"/> or
/// <see cref="InvalidOperationException"/>.
/// </summary>
/// <remarks>
/// <para>
/// Exceptions are classified by <em>what the failure means to the caller</em>,
/// never by which layer or bounded context raised them. Three questions decide
/// the type, in order:
/// </para>
/// <list type="number">
///   <item>
///   Is the request itself invalid? Then <see cref="DomainException"/> (400).
///   Nothing about the current state of the system caused the failure: correct
///   the request and it would succeed. Examples: a malformed email, an empty
///   name, an unknown document type, an empty file.
///   </item>
///   <item>
///   Is the request valid, but blocked by current business state? Then
///   <see cref="BusinessRuleViolationException"/> (409). Examples: a duplicate
///   email, an inactive organization, a submission that is already published.
///   </item>
///   <item>
///   Does the resource not exist, or is it deliberately invisible to this
///   caller? Then <see cref="NotFoundException"/> (404).
///   </item>
/// </list>
/// <para>
/// Anything else is an unexpected failure and surfaces as a 500. New categories
/// are added only when a real requirement demands one.
/// </para>
/// <para>
/// Intentionally minimal: a message and nothing else. Error codes, severities
/// and categories are deferred until something needs them. It takes no
/// framework dependency and knows nothing about HTTP, validation or logging —
/// the status codes above describe how the API layer maps these types, not
/// something the types themselves are aware of.
/// </para>
/// </remarks>
public class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }
}
