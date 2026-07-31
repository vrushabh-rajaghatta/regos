namespace RegOS.Product.Domain.Product;

public static class MedicinalProductErrors
{
    public const string TenantRequired =
        "A medicinal product must belong to a tenant.";

    public const string GlobalProductRequired =
        "A medicinal product must localise a global product.";

    public const string CountryRequired =
        "A medicinal product must name the country it is marketed in.";

    public const string StatusDateRequired =
        "A status date is required.";
}
