namespace RegOS.Product.Domain.Product;

public static class ProductErrors
{
    public const string NameRequired = "Product name is required.";

    public const string NameTooLong =
        "Product name must be 200 characters or fewer.";

    public const string CodeRequired = "Product code is required.";

    public const string CodeTooLong =
        "Product code must be 50 characters or fewer.";

    public const string CodeInvalidCharacters =
        "Product code may contain only letters, digits, dashes and underscores.";
}
