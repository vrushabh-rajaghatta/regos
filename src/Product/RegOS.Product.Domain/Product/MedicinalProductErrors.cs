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

    public const string LanguageRequired =
        "A language is required.";

    public const string LanguageNotRecognised =
        "A language is a two-letter ISO 639-1 code, such as en or fr.";

    public const string TradeNameRequired =
        "A trade name is required.";

    public static readonly string TradeNameTooLong =
        $"A trade name must be {TradeName.NameMaxLength} characters or fewer.";

    /// <summary>
    /// The deliberate opposite of the tier's own rule. Two brand names in one
    /// language for one market means one of them is wrong; two market presences
    /// in one country is an ordinary business fact.
    /// </summary>
    public const string TradeNameLanguageAlreadyRecorded =
        "This market already has a trade name in that language.";

    public const string TradeNameNotFound =
        "Trade name does not exist.";
}
