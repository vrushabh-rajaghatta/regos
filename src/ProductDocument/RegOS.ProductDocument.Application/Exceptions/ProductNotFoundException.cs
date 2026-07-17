namespace RegOS.ProductDocument.Application.Exceptions;

/// <summary>
/// Raised when the product named in the route does not exist. The product is
/// the addressed resource, so this maps to 404.
/// </summary>
public sealed class ProductNotFoundException : Exception
{
    public ProductNotFoundException(string message)
        : base(message)
    {
    }
}
