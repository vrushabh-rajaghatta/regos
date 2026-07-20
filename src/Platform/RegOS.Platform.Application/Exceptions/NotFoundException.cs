namespace RegOS.Platform.Application.Exceptions;

/// <summary>
/// Raised when a query or command targets a record that does not exist, or that
/// the caller's organization cannot see. Queries signal "not found" explicitly
/// rather than returning null, so the API has one consistent contract and
/// nullable handling does not leak through the application layer. Mapped to
/// HTTP 404.
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message)
    {
    }
}
