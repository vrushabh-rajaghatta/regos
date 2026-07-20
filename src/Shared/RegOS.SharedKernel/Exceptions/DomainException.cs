namespace RegOS.SharedKernel.Exceptions;

/// <summary>
/// Thrown when a domain (business) rule is violated — for example creating an
/// organization without a legal name, or publishing a submission twice. Signals
/// a broken invariant, not a technical or programming fault, which is why the
/// domain raises it in preference to <see cref="ArgumentException"/> or
/// <see cref="InvalidOperationException"/>.
/// </summary>
/// <remarks>
/// Intentionally minimal for now: a message and nothing else. Error codes,
/// severities, categories and the like are deferred until a real requirement
/// appears. It takes no framework dependency and knows nothing about HTTP,
/// validation or logging.
/// </remarks>
public class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }
}
