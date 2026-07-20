namespace RegOS.SharedKernel.Exceptions;

/// <summary>
/// The requested resource does not exist in the caller's visible world. Mapped
/// to HTTP 404.
/// </summary>
/// <remarks>
/// Deliberately does not distinguish "no such record" from "outside your
/// tenant": the caller gets the same contract either way, so the existence of
/// another organization's data is never disclosed. Queries raise this rather
/// than returning null, so the API has one consistent contract and nullable
/// handling does not leak through the application layer.
/// </remarks>
public class NotFoundException : DomainException
{
    public NotFoundException(string message)
        : base(message)
    {
    }
}
