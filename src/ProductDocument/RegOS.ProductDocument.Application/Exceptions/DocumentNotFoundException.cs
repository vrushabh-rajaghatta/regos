namespace RegOS.ProductDocument.Application.Exceptions;

/// <summary>
/// Raised when the document named in the route does not exist (or does not
/// belong to the product in the route). The document is the addressed
/// resource for lifecycle actions, so this maps to 404.
/// </summary>
public sealed class DocumentNotFoundException : Exception
{
    public DocumentNotFoundException(string message)
        : base(message)
    {
    }
}
